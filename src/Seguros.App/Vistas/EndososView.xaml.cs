using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;
using Seguros.Domain.Exceptions;

namespace Seguros.App.Vistas;

public partial class EndososView : UserControl
{
    private readonly ObservableCollection<Endoso> _filas = new();

    public EndososView()
    {
        InitializeComponent();
        Grid.ItemsSource = _filas;
        Loaded += async (_, _) => await CargarPolizas();
    }

    private async Task CargarPolizas() => CmbPoliza.ItemsSource = await AppServices.Polizas.Consultar();

    private async void BtnActualizarPolizas_Click(object sender, RoutedEventArgs e) => await CargarPolizas();

    private async void CmbPoliza_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _filas.Clear();
        if (CmbPoliza.SelectedItem is not Poliza p) return;
        foreach (var endoso in await AppServices.Endosos.HistorialEndosos(p.Id))
            _filas.Add(endoso);
    }

    private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {
        if (CmbPoliza.SelectedItem is not Poliza p)
        {
            MessageBox.Show("Elegí primero una póliza.", "Falta seleccionar", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var fecha = DateOnly.FromDateTime(DpFecha.SelectedDate ?? DateTime.Today);
            await AppServices.Endosos.AltaEndoso(p.Id, fecha, TxtTipo.Text.Trim(), TxtDetalle.Text.Trim());
            TxtTipo.Clear(); TxtDetalle.Clear();
            CmbPoliza_SelectionChanged(sender, null!);
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo agregar el endoso", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnAnular_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Endoso endoso) return;
        await AppServices.Endosos.AnularEndoso(endoso.Id, TxtMotivoAnulacion.Text.Trim());
        TxtMotivoAnulacion.Clear();
        CmbPoliza_SelectionChanged(sender, null!);
    }
}
