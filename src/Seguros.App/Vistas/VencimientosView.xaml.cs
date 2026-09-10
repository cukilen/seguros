using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Seguros.Domain.Entities;

namespace Seguros.App.Vistas;

public partial class VencimientosView : UserControl
{
    private readonly ObservableCollection<Poliza> _filas = new();

    public VencimientosView()
    {
        InitializeComponent();
        Grid.ItemsSource = _filas;
        Loaded += async (_, _) =>
        {
            CmbCompania.ItemsSource = new Compania?[] { null }.Concat(await AppServices.Companias.ListarCompanias());
            await Mostrar(proximas: true);
        };
    }

    private async Task Mostrar(bool proximas)
    {
        var companiaId = (CmbCompania.SelectedItem as Compania)?.Id;
        var lista = proximas
            ? await AppServices.Vencimientos.ListarProximasAVencer(int.TryParse(TxtDias.Text, out var d) ? d : 30, AppServices.ProductorActual.Id, companiaId)
            : await AppServices.Vencimientos.ListarVencidas(AppServices.ProductorActual.Id, companiaId);

        _filas.Clear();
        foreach (var p in lista) _filas.Add(p);
    }

    private async void BtnProximas_Click(object sender, RoutedEventArgs e) => await Mostrar(proximas: true);

    private async void BtnVencidas_Click(object sender, RoutedEventArgs e) => await Mostrar(proximas: false);

    private async void BtnMarcarGestionado_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Poliza p)
        {
            Dialogos.Error("Seleccioná primero una póliza de la lista.", "Nada seleccionado");
            return;
        }

        await AppServices.Vencimientos.MarcarComoGestionado(p.Id);
        await Mostrar(proximas: true);
    }
}
