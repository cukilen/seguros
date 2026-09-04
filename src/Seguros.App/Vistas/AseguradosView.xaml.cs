using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;
using Seguros.Domain.Exceptions;

namespace Seguros.App.Vistas;

public partial class AseguradosView : UserControl
{
    private readonly ObservableCollection<Asegurado> _filas = new();

    public AseguradosView()
    {
        InitializeComponent();
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
        try
        {
            await AppServices.Asegurados.AltaAsegurado(AppServices.ProductorActual.Id, TxtNombre.Text.Trim(),
                TxtDocumento.Text.Trim(), TxtTelefono.Text.Trim(), TxtEmail.Text.Trim(), TxtDomicilio.Text.Trim());
            TxtNombre.Clear(); TxtDocumento.Clear(); TxtTelefono.Clear(); TxtEmail.Clear(); TxtDomicilio.Clear();
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo agregar", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        if (Grid.SelectedItem is not Asegurado a) return;
        await AppServices.Asegurados.InactivarAsegurado(a.Id);
        await Recargar();
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Asegurado a) return;
        try
        {
            await AppServices.Asegurados.EliminarAsegurado(a.Id);
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo eliminar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        GridPolizas.ItemsSource = Grid.SelectedItem is Asegurado a
            ? await AppServices.Asegurados.ObtenerPolizasDeAsegurado(a.Id)
            : null;
    }
}
