using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.App.Vistas;

public partial class PolizasView : UserControl
{
    private readonly ObservableCollection<Poliza> _filas = new();

    public PolizasView()
    {
        InitializeComponent();
        Grid.ItemsSource = _filas;
        CmbRamo.ItemsSource = Enum.GetValues<Ramo>();

        CmbFiltroRamo.ItemsSource = new object[] { "(todos)" }.Concat(Enum.GetValues<Ramo>().Cast<object>());
        CmbFiltroRamo.SelectedIndex = 0;

        Loaded += async (_, _) => await CargarCombosYGrilla();
    }

    private async Task CargarCombosYGrilla()
    {
        CmbAsegurado.ItemsSource = await AppServices.Asegurados.Buscar(AppServices.ProductorActual.Id, string.Empty);
        var companias = await AppServices.Companias.ListarCompanias();
        CmbCompania.ItemsSource = companias;
        CmbFiltroCompania.ItemsSource = new Compania?[] { null }.Concat(companias);
        await Recargar();
    }

    private async Task Recargar()
    {
        Ramo? ramo = CmbFiltroRamo.SelectedItem is Ramo r ? r : null;
        var companiaId = (CmbFiltroCompania.SelectedItem as Compania)?.Id;

        _filas.Clear();
        foreach (var p in await AppServices.Polizas.Consultar(companiaId: companiaId, ramo: ramo))
            _filas.Add(p);
    }

    private async void Filtro_Changed(object sender, SelectionChangedEventArgs e) => await Recargar();

    private async void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
    {
        CmbFiltroRamo.SelectedIndex = 0;
        CmbFiltroCompania.SelectedIndex = 0;
        await Recargar();
    }

    private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var asegurado = (Asegurado)CmbAsegurado.SelectedItem;
            var compania = (Compania)CmbCompania.SelectedItem;
            var ramo = (Ramo)CmbRamo.SelectedItem;
            var desde = DateOnly.FromDateTime(DpDesde.SelectedDate!.Value);
            var hasta = DateOnly.FromDateTime(DpHasta.SelectedDate!.Value);
            var prima = decimal.Parse(TxtPrima.Text.Trim(), CultureInfo.GetCultureInfo("es-AR"));

            await AppServices.Polizas.AltaPoliza(AppServices.ProductorActual.Id, asegurado.Id, compania.Id, ramo,
                TxtNumero.Text.Trim(), desde, hasta, prima, ChkFlota.IsChecked == true);

            TxtNumero.Clear(); TxtPrima.Clear(); ChkFlota.IsChecked = false;
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo agregar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show("Revisá que todos los campos estén completos y sean válidos.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnAnular_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Poliza p) return;
        try
        {
            await AppServices.Polizas.AnularPoliza(p.Id, TxtMotivoAnulacion.Text.Trim());
            TxtMotivoAnulacion.Clear();
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo anular", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnRenovar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Poliza p) return;
        try
        {
            var nuevaVigenciaHasta = DateOnly.FromDateTime(DpNuevaVigenciaHasta.SelectedDate!.Value);
            var nuevaPrima = decimal.Parse(TxtNuevaPrima.Text.Trim(), CultureInfo.GetCultureInfo("es-AR"));

            await AppServices.Polizas.RenovarPoliza(p.Id, TxtNuevoNumero.Text.Trim(), p.VigenciaHasta, nuevaVigenciaHasta, nuevaPrima);

            TxtNuevoNumero.Clear(); TxtNuevaPrima.Clear();
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo renovar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show("Completá número, nueva vigencia y prima para renovar.", "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
