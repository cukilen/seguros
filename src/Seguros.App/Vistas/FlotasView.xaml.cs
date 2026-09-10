using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Seguros.Data.Services;
using Seguros.Domain.Entities;
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
        var polizasAutos = (await AppServices.Polizas.Consultar())
            .Where(p => PolizaService.EsRamoAutos(p.Ramo))
            .Select(p => new PolizaItem(p))
            .ToList();
        CmbPoliza.ItemsSource = polizasAutos;
        CmbPoliza.DisplayMemberPath = nameof(PolizaItem.Texto);
    }

    private async void BtnActualizarPolizas_Click(object sender, RoutedEventArgs e) => await CargarPolizas();

    private async void BtnMarcarComoFlota_Click(object sender, RoutedEventArgs e)
    {
        if (CmbPoliza.SelectedItem is not PolizaItem item)
        {
            Dialogos.Error("Seleccioná primero una póliza de la lista.", "Nada seleccionado");
            return;
        }
        try
        {
            await AppServices.Flotas.MarcarComoFlota(item.Poliza.Id);
            await CargarPolizas();
        }
        catch (ReglaDeNegocioException ex)
        {
            Dialogos.Error(ex.Message, "No se pudo marcar como flota");
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
            Dialogos.Error("Elegí primero una póliza de flota.", "Nada seleccionado");
            return;
        }
        if (string.IsNullOrWhiteSpace(TxtPatente.Text))
        {
            Dialogos.Error("Ingresá la patente de la unidad.", "Faltan datos");
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
            Dialogos.Error(ex.Message, "No se pudo agregar la unidad");
        }
    }

    private async void BtnBajaUnidad_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not UnidadFlota u)
        {
            Dialogos.Error("Seleccioná primero una unidad de la lista.", "Nada seleccionado");
            return;
        }
        if (!Dialogos.Confirmar($"¿Dar de baja la unidad con patente \"{u.Patente}\"?", "Confirmar baja"))
            return;

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
