using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;

namespace Seguros.App.Vistas;

public partial class ComisionesView : UserControl
{
    private readonly ObservableCollection<Liquidacion> _filas = new();

    public ComisionesView()
    {
        InitializeComponent();
        CmbRamo.ItemsSource = Enum.GetValues<Ramo>();
        Grid.ItemsSource = _filas;
        Loaded += async (_, _) =>
        {
            CmbCompania.ItemsSource = await AppServices.Companias.ListarCompanias();
            await Recargar();
        };
    }

    private async Task Recargar()
    {
        _filas.Clear();
        foreach (var l in await AppServices.Comisiones.HistorialLiquidaciones(AppServices.ProductorActual.Id))
            _filas.Add(l);
    }

    private async void BtnGuardarComision_Click(object sender, RoutedEventArgs e)
    {
        if (CmbCompania.SelectedItem is not Compania compania || CmbRamo.SelectedItem is not Ramo ramo)
        {
            MessageBox.Show("Elegí compañía y ramo.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(TxtPorcentaje.Text.Trim(), NumberStyles.Number, CultureInfo.GetCultureInfo("es-AR"), out var porcentaje))
        {
            MessageBox.Show("El porcentaje ingresado no es válido.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await AppServices.Comisiones.ConfigurarComision(compania.Id, ramo, porcentaje);
        MessageBox.Show("Porcentaje de comisión guardado.", "Listo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void BtnGenerarLiquidacion_Click(object sender, RoutedEventArgs e)
    {
        if (DpDesde.SelectedDate is null || DpHasta.SelectedDate is null)
        {
            MessageBox.Show("Elegí el período (desde/hasta).", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var liquidacion = await AppServices.Comisiones.GenerarLiquidacion(AppServices.ProductorActual.Id,
            DateOnly.FromDateTime(DpDesde.SelectedDate.Value), DateOnly.FromDateTime(DpHasta.SelectedDate.Value));

        await Recargar();
        MessageBox.Show($"Liquidación generada por un total de {liquidacion.MontoTotal:C}.", "Listo", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
