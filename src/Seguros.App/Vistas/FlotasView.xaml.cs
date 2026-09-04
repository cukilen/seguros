using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.App.Vistas;

public partial class FlotasView : UserControl
{
    private readonly ObservableCollection<UnidadFlota> _filas = new();

    public FlotasView()
    {
        InitializeComponent();
        Grid.ItemsSource = _filas;
        Loaded += async (_, _) => await CargarPolizas();
    }

    private async Task CargarPolizas()
    {
        var polizasAutos = (await AppServices.Polizas.Consultar(ramo: Ramo.Autos))
            .Select(p => new PolizaItem(p))
            .ToList();
        CmbPoliza.ItemsSource = polizasAutos;
        CmbPoliza.DisplayMemberPath = nameof(PolizaItem.Texto);
    }

    private async void BtnActualizarPolizas_Click(object sender, RoutedEventArgs e) => await CargarPolizas();

    private async void BtnMarcarComoFlota_Click(object sender, RoutedEventArgs e)
    {
        if (CmbPoliza.SelectedItem is not PolizaItem item) return;
        try
        {
            await AppServices.Flotas.MarcarComoFlota(item.Poliza.Id);
            await CargarPolizas();
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo marcar como flota", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void CmbPoliza_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _filas.Clear();
        if (CmbPoliza.SelectedItem is not PolizaItem item) return;
        foreach (var u in await AppServices.Flotas.ListarUnidades(item.Poliza.Id))
            _filas.Add(u);
    }

    private async void BtnAgregarUnidad_Click(object sender, RoutedEventArgs e)
    {
        if (CmbPoliza.SelectedItem is not PolizaItem item)
        {
            MessageBox.Show("Elegí primero una póliza de flota.", "Falta seleccionar", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await AppServices.Flotas.AltaUnidad(item.Poliza.Id, TxtPatente.Text.Trim(), TxtMarca.Text.Trim(), TxtModelo.Text.Trim(), TxtUso.Text.Trim());
            TxtPatente.Clear(); TxtMarca.Clear(); TxtModelo.Clear(); TxtUso.Clear();
            CmbPoliza_SelectionChanged(sender, null!);
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo agregar la unidad", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnBajaUnidad_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not UnidadFlota u) return;
        await AppServices.Flotas.BajaUnidad(u.Id, DateOnly.FromDateTime(DateTime.Today));
        CmbPoliza_SelectionChanged(sender, null!);
    }

    private class PolizaItem
    {
        public PolizaItem(Poliza p) => Poliza = p;
        public Poliza Poliza { get; }
        public string Texto => $"{Poliza.Numero} - {(Poliza.EsFlota ? "Flota" : "No marcada como flota")}";
    }
}
