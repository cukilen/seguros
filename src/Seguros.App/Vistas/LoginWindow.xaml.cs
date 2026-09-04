using System.Windows;
using Seguros.Domain.Exceptions;

namespace Seguros.App.Vistas;

public partial class LoginWindow : Window
{
    private bool _modoCrearProductor;

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void BtnAlternar_Click(object sender, RoutedEventArgs e)
    {
        _modoCrearProductor = !_modoCrearProductor;
        PanelNombre.Visibility = _modoCrearProductor ? Visibility.Visible : Visibility.Collapsed;
        BtnAccionPrincipal.Content = _modoCrearProductor ? "Crear productor" : "Ingresar";
        BtnAlternar.Content = _modoCrearProductor ? "Ya tengo usuario, iniciar sesión" : "¿Primera vez? Crear productor";
        TxtError.Text = string.Empty;
    }

    private async void BtnAccionPrincipal_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Text = string.Empty;
        var usuario = TxtUsuario.Text.Trim();
        var password = TxtPassword.Password;

        try
        {
            if (_modoCrearProductor)
            {
                var productor = await AppServices.Productores.AltaProductor(TxtNombre.Text.Trim(), usuario, password);
                AppServices.ProductorActual = productor;
            }
            else
            {
                var productor = await AppServices.Productores.Login(usuario, password);
                if (productor is null)
                {
                    TxtError.Text = "Usuario o contraseña incorrectos.";
                    return;
                }
                AppServices.ProductorActual = productor;
            }

            DialogResult = true;
            Close();
        }
        catch (ReglaDeNegocioException ex)
        {
            TxtError.Text = ex.Message;
        }
    }
}
