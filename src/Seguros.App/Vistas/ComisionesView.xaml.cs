using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;

namespace Seguros.App.Vistas;

public partial class ComisionesView : UserControl
{
    private readonly ObservableCollection<Liquidacion> _filas = new();

    public ComisionesView()
    {
        InitializeComponent();
        Grid.ItemsSource = _filas;
        Loaded += async (_, _) =>
        {
            CmbCompania.ItemsSource = await AppServices.Companias.ListarCompanias();
            CmbRamo.ItemsSource = await AppServices.Ramos.Listar();
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
            Dialogos.Error("Elegí compañía y ramo.", "Faltan datos");
            return;
        }

        if (!decimal.TryParse(TxtPorcentaje.Text.Trim(), NumberStyles.Number, CultureInfo.GetCultureInfo("es-AR"), out var porcentaje))
        {
            Dialogos.Error("El porcentaje ingresado no es válido.", "Dato inválido");
            return;
        }

        await AppServices.Comisiones.ConfigurarComision(compania.Id, ramo.Id, porcentaje);
        Dialogos.Info("Porcentaje de comisión guardado.");
    }

    private async void BtnGenerarLiquidacion_Click(object sender, RoutedEventArgs e)
    {
        if (DpDesde.SelectedDate is null || DpHasta.SelectedDate is null)
        {
            Dialogos.Error("Elegí el período (desde/hasta).", "Faltan datos");
            return;
        }

        var desde = DateOnly.FromDateTime(DpDesde.SelectedDate.Value);
        var hasta = DateOnly.FromDateTime(DpHasta.SelectedDate.Value);
        if (!Dialogos.Confirmar($"¿Generar la liquidación del período {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}? Si ya generaste una liquidación para este período, se creará una nueva de todos modos.", "Confirmar liquidación"))
            return;

        var liquidacion = await AppServices.Comisiones.GenerarLiquidacion(AppServices.ProductorActual.Id, desde, hasta);

        await Recargar();
        Dialogos.Info($"Liquidación generada por un total de {liquidacion.MontoTotal:C}.");
    }
}
