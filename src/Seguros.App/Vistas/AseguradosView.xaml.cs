using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.App.Vistas;

public partial class AseguradosView : UserControl
{
    private readonly ObservableCollection<Asegurado> _filas = new();

    public AseguradosView()
    {
        InitializeComponent();
        CmbTipoDocumento.ItemsSource = Enum.GetValues<TipoDocumento>();
        Grid.ItemsSource = _filas;
        Loaded += async (_, _) => await Recargar();
    }

    private async Task Recargar(IEnumerable<Asegurado>? datos = null)
    {
        _filas.Clear();
        var lista = datos ?? await AppServices.Asegurados.Buscar(AppServices.ProductorActual.Id, string.Empty);
        foreach (var a in lista) _filas.Add(a);
    }

    private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            Dialogos.Error("Ingresá el nombre del asegurado.", "Faltan datos");
            return;
        }
        if (CmbTipoDocumento.SelectedItem is null != string.IsNullOrWhiteSpace(TxtDocumento.Text))
        {
            Dialogos.Error("Indicá tipo y número de documento juntos, o dejá ambos vacíos.", "Datos incompletos");
            return;
        }

        try
        {
            TipoDocumento? tipoDocumento = CmbTipoDocumento.SelectedItem is TipoDocumento t ? t : null;
            var nroDocumento = string.IsNullOrWhiteSpace(TxtDocumento.Text) ? null : TxtDocumento.Text.Trim();

            await AppServices.Asegurados.AltaAsegurado(AppServices.ProductorActual.Id, TxtNombre.Text.Trim(),
                tipoDocumento, nroDocumento, TxtTelefono.Text.Trim(), TxtEmail.Text.Trim(), TxtDomicilio.Text.Trim());
            TxtNombre.Clear(); TxtDocumento.Clear(); CmbTipoDocumento.SelectedIndex = -1;
            TxtTelefono.Clear(); TxtEmail.Clear(); TxtDomicilio.Clear();
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            Dialogos.Error(ex.Message);
        }
    }

    private async void BtnBuscar_Click(object sender, RoutedEventArgs e) =>
        await Recargar(await AppServices.Asegurados.Buscar(AppServices.ProductorActual.Id, TxtBusqueda.Text.Trim()));

    private async void BtnMostrarTodos_Click(object sender, RoutedEventArgs e)
    {
        TxtBusqueda.Clear();
        await Recargar();
    }

    private async void BtnInactivar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Asegurado a)
        {
            Dialogos.Error("Seleccioná primero un asegurado de la lista.", "Nada seleccionado");
            return;
        }
        if (!Dialogos.Confirmar($"¿Inactivar a \"{a.Nombre}\"? Se puede reactivar más adelante.", "Confirmar inactivación"))
            return;

        await AppServices.Asegurados.InactivarAsegurado(a.Id);
        await Recargar();
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Asegurado a)
        {
            Dialogos.Error("Seleccioná primero un asegurado de la lista.", "Nada seleccionado");
            return;
        }
        if (!Dialogos.Confirmar($"¿Eliminar definitivamente a \"{a.Nombre}\"? Esta acción no se puede deshacer.", "Confirmar eliminación"))
            return;

        try
        {
            await AppServices.Asegurados.EliminarAsegurado(a.Id);
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            Dialogos.Error(ex.Message, "No se pudo eliminar");
        }
    }

    private async void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        GridPolizas.ItemsSource = Grid.SelectedItem is Asegurado a
            ? await AppServices.Asegurados.ObtenerPolizasDeAsegurado(a.Id)
            : null;
    }
}
