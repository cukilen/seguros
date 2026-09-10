using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;
using Seguros.Domain.Exceptions;

namespace Seguros.App.Vistas;

public partial class CompaniasView : UserControl
{
    private readonly ObservableCollection<CompaniaFila> _filas = new();

    public CompaniasView()
    {
        InitializeComponent();
        Grid.ItemsSource = _filas;
        Loaded += async (_, _) =>
        {
            await CargarRamos();
            await Recargar();
        };
    }

    private async Task CargarRamos() => LstRamos.ItemsSource = await AppServices.Ramos.Listar();

    private async Task Recargar()
    {
        _filas.Clear();
        foreach (var c in await AppServices.Companias.ListarCompanias())
            _filas.Add(new CompaniaFila(c));
    }

    private async void BtnAgregarRamo_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNuevoRamo.Text))
        {
            Dialogos.Error("Ingresá el nombre del ramo.", "Faltan datos");
            return;
        }

        try
        {
            await AppServices.Ramos.AltaRamo(TxtNuevoRamo.Text.Trim());
            TxtNuevoRamo.Clear();
            await CargarRamos();
        }
        catch (ReglaDeNegocioException ex)
        {
            Dialogos.Error(ex.Message, "No se pudo agregar el ramo");
        }
    }

    private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            Dialogos.Error("Ingresá el nombre de la compañía.", "Faltan datos");
            return;
        }

        try
        {
            var ramoIds = LstRamos.SelectedItems.Cast<Ramo>().Select(r => r.Id).ToList();
            await AppServices.Companias.AltaCompania(TxtNombre.Text.Trim(), ramoIds, TxtTelefono.Text.Trim(), TxtEmail.Text.Trim());
            TxtNombre.Clear(); TxtTelefono.Clear(); TxtEmail.Clear(); LstRamos.SelectedItems.Clear();
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            Dialogos.Error(ex.Message);
        }
    }

    private async void BtnInactivar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not CompaniaFila fila)
        {
            Dialogos.Error("Seleccioná primero una compañía de la lista.", "Nada seleccionado");
            return;
        }
        if (!Dialogos.Confirmar($"¿Inactivar la compañía \"{fila.Nombre}\"? Dejará de estar disponible para nuevas pólizas, pero se puede reactivar más adelante.", "Confirmar inactivación"))
            return;

        await AppServices.Companias.InactivarCompania(fila.Id);
        await Recargar();
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not CompaniaFila fila)
        {
            Dialogos.Error("Seleccioná primero una compañía de la lista.", "Nada seleccionado");
            return;
        }
        if (!Dialogos.Confirmar($"¿Eliminar definitivamente la compañía \"{fila.Nombre}\"? Esta acción no se puede deshacer.", "Confirmar eliminación"))
            return;

        try
        {
            await AppServices.Companias.EliminarCompania(fila.Id);
            await Recargar();
        }
        catch (ReglaDeNegocioException ex)
        {
            Dialogos.Error(ex.Message, "No se pudo eliminar");
        }
    }

    private class CompaniaFila
    {
        public CompaniaFila(Compania c)
        {
            Id = c.Id; Nombre = c.Nombre; Telefono = c.Telefono; Email = c.Email; Activa = c.Activa;
            RamosTexto = string.Join(", ", c.RamosOperados.Select(r => r.Ramo.Nombre));
        }

        public int Id { get; }
        public string Nombre { get; }
        public string? Telefono { get; }
        public string? Email { get; }
        public bool Activa { get; }
        public string RamosTexto { get; }
    }
}
