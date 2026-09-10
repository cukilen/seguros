using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;
using Seguros.Domain.Exceptions;

namespace Seguros.App.Vistas;

public partial class PolizasView : UserControl
{
    private readonly ObservableCollection<Poliza> _filas = new();

    public PolizasView()
    {
        InitializeComponent();
        Grid.ItemsSource = _filas;

        Loaded += async (_, _) => await CargarCombosYGrilla();
    }

    private async Task CargarCombosYGrilla()
    {
        CmbAsegurado.ItemsSource = await AppServices.Asegurados.Buscar(AppServices.ProductorActual.Id, string.Empty);
        var companias = await AppServices.Companias.ListarCompanias();
        CmbCompania.ItemsSource = companias;
        CmbFiltroCompania.ItemsSource = new Compania?[] { null }.Concat(companias);

        var ramos = await AppServices.Ramos.Listar();
        CmbRamo.ItemsSource = ramos;
        CmbFiltroRamo.ItemsSource = new Ramo?[] { null }.Concat(ramos);

        await Recargar();
    }

    private async Task Recargar()
    {
        var ramoId = (CmbFiltroRamo.SelectedItem as Ramo)?.Id;
        var companiaId = (CmbFiltroCompania.SelectedItem as Compania)?.Id;

        _filas.Clear();
        foreach (var p in await AppServices.Polizas.Consultar(companiaId: companiaId, ramoId: ramoId))
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
        if (CmbAsegurado.SelectedItem is null || CmbCompania.SelectedItem is null || CmbRamo.SelectedItem is null)
        {
            Dialogos.Error("Elegí asegurado, compañía y ramo antes de agregar la póliza.", "Faltan datos");
            return;
        }
        if (DpDesde.SelectedDate is null || DpHasta.SelectedDate is null)
        {
            Dialogos.Error("Completá las fechas de vigencia (desde y hasta).", "Faltan datos");
            return;
        }
        if (string.IsNullOrWhiteSpace(TxtNumero.Text))
        {
            Dialogos.Error("Ingresá el número de póliza.", "Faltan datos");
            return;
        }
        if (!decimal.TryParse(TxtPrima.Text.Trim(), NumberStyles.Number, CultureInfo.GetCultureInfo("es-AR"), out var prima))
        {
            Dialogos.Error("La prima ingresada no es un número válido.", "Dato inválido");
            return;
        }

        try
        {
            var asegurado = (Asegurado)CmbAsegurado.SelectedItem;
            var compania = (Compania)CmbCompania.SelectedItem;
            var ramo = (Ramo)CmbRamo.SelectedItem;
            var desde = DateOnly.FromDateTime(DpDesde.SelectedDate.Value);
            var hasta = DateOnly.FromDateTime(DpHasta.SelectedDate.Value);

            await AppServices.Polizas.AltaPoliza(AppServices.ProductorActual.Id, asegurado.Id, compania.Id, ramo.Id,
                TxtNumero.Text.Trim(), desde, hasta, prima, ChkFlota.IsChecked == true);

            TxtNumero.Clear(); TxtPrima.Clear(); ChkFlota.IsChecked = false;
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            Dialogos.Error(ex.Message);
        }
    }

    private async void BtnAnular_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Poliza p)
        {
            Dialogos.Error("Seleccioná primero una póliza de la lista.", "Nada seleccionado");
            return;
        }
        if (string.IsNullOrWhiteSpace(TxtMotivoAnulacion.Text))
        {
            Dialogos.Error("Indicá el motivo de la anulación.", "Faltan datos");
            return;
        }
        if (!Dialogos.Confirmar($"¿Anular la póliza N.º {p.Numero}? Esta acción no se puede deshacer.", "Confirmar anulación"))
            return;

        try
        {
            await AppServices.Polizas.AnularPoliza(p.Id, TxtMotivoAnulacion.Text.Trim());
            TxtMotivoAnulacion.Clear();
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            Dialogos.Error(ex.Message, "No se pudo anular");
        }
    }

    private async void BtnRenovar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Poliza p)
        {
            Dialogos.Error("Seleccioná primero una póliza de la lista.", "Nada seleccionado");
            return;
        }
        if (string.IsNullOrWhiteSpace(TxtNuevoNumero.Text))
        {
            Dialogos.Error("Ingresá el número de la póliza renovada.", "Faltan datos");
            return;
        }
        if (DpNuevaVigenciaHasta.SelectedDate is null)
        {
            Dialogos.Error("Elegí la nueva fecha de fin de vigencia.", "Faltan datos");
            return;
        }
        if (!decimal.TryParse(TxtNuevaPrima.Text.Trim(), NumberStyles.Number, CultureInfo.GetCultureInfo("es-AR"), out var nuevaPrima))
        {
            Dialogos.Error("La nueva prima ingresada no es un número válido.", "Dato inválido");
            return;
        }

        try
        {
            var nuevaVigenciaHasta = DateOnly.FromDateTime(DpNuevaVigenciaHasta.SelectedDate.Value);
            await AppServices.Polizas.RenovarPoliza(p.Id, TxtNuevoNumero.Text.Trim(), p.VigenciaHasta, nuevaVigenciaHasta, nuevaPrima);

            TxtNuevoNumero.Clear(); TxtNuevaPrima.Clear();
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            Dialogos.Error(ex.Message, "No se pudo renovar");
        }
    }
}
