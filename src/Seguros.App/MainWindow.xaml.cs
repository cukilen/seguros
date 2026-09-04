using System.Windows;

namespace Seguros.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = $"Gestión Integral de Seguros - {AppServices.ProductorActual.Nombre}";
    }
}
