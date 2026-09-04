using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.App.Vistas;

public partial class CompaniasView : UserControl
{
    private readonly ObservableCollection<CompaniaFila> _filas = new();

    public CompaniasView()
    {
        InitializeComponent();
        LstRamos.ItemsSource = Enum.GetValues<Ramo>();
        Grid.ItemsSource = _filas;
        Loaded += async (_, _) => await Recargar();
    }

    private async Task Recargar()
    {
        _filas.Clear();
        foreach (var c in await AppServices.Companias.ListarCompanias())
            _filas.Add(new CompaniaFila(c));
    }

    private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var ramos = LstRamos.SelectedItems.Cast<Ramo>().ToList();
            await AppServices.Companias.AltaCompania(TxtNombre.Text.Trim(), ramos, TxtTelefono.Text.Trim(), TxtEmail.Text.Trim());
            TxtNombre.Clear(); TxtTelefono.Clear(); TxtEmail.Clear(); LstRamos.SelectedItems.Clear();
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo agregar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnInactivar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not CompaniaFila fila) return;
        await AppServices.Companias.InactivarCompania(fila.Id);
        await Recargar();
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not CompaniaFila fila) return;
        try
        {
            await AppServices.Companias.EliminarCompania(fila.Id);
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo eliminar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private class CompaniaFila
    {
        public CompaniaFila(Compania c)
        {
            Id = c.Id; Nombre = c.Nombre; Telefono = c.Telefono; Email = c.Email; Activa = c.Activa;
            RamosTexto = string.Join(", ", c.RamosOperados.Select(r => r.Ramo));
        }

        public int Id { get; }
        public string Nombre { get; }
        public string? Telefono { get; }
        public string? Email { get; }
        public bool Activa { get; }
        public string RamosTexto { get; }
    }
}
