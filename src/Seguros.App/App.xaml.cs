using System.Windows;
using System.Windows.Threading;
using Seguros.App.Vistas;

namespace Seguros.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += App_DispatcherUnhandledException;

        // Sin esto, WPF cierra toda la app apenas se cierra la ventana de login
        // (la toma como "última ventana abierta" porque todavía no hay MainWindow).
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        AppServices.Inicializar();

        var login = new LoginWindow();
        var ok = login.ShowDialog();

        if (ok != true)
        {
            Shutdown();
            return;
        }

        var main = new MainWindow();
        MainWindow = main;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        main.Show();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Ocurrió un error inesperado:{Environment.NewLine}{Environment.NewLine}{e.Exception}",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
