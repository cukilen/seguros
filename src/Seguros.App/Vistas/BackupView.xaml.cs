using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Seguros.App.Vistas;

public partial class BackupView : UserControl
{
    public BackupView() => InitializeComponent();

    private void BtnBackup_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new SaveFileDialog
        {
            Filter = "Base de datos SQLite (*.db)|*.db",
            FileName = $"seguros-backup-{DateTime.Now:yyyyMMdd-HHmm}.db"
        };
        if (dialogo.ShowDialog() != true) return;

        try
        {
            AppServices.Backup.HacerBackup(dialogo.FileName);
            TxtResultado.Text = $"Backup generado correctamente en: {dialogo.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo generar el backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
