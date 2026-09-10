using System.Windows;

namespace Seguros.App;

/// <summary>Wrappers consistentes para diálogos de confirmación, error e información.</summary>
public static class Dialogos
{
    public static bool Confirmar(string mensaje, string titulo = "Confirmar acción") =>
        MessageBox.Show(mensaje, titulo, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    public static void Error(string mensaje, string titulo = "No se pudo completar la acción") =>
        MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Warning);

    public static void Info(string mensaje, string titulo = "Listo") =>
        MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
}
