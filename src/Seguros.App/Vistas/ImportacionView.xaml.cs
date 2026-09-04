using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Seguros.Data.Importacion;

namespace Seguros.App.Vistas;

public partial class ImportacionView : UserControl
{
    private const string NoImportar = "(no importar)";

    private readonly ObservableCollection<ItemImportacionFila> _filas = new();
    private List<Dictionary<string, string>> _filasCrudas = new();

    public ImportacionView()
    {
        InitializeComponent();
        Grid.ItemsSource = _filas;
    }

    private void BtnElegirArchivo_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog { Filter = "Archivos de listado (*.xlsx;*.pdf)|*.xlsx;*.pdf" };
        if (dialogo.ShowDialog() != true) return;

        TxtArchivo.Text = Path.GetFileName(dialogo.FileName);
        TxtMensaje.Text = string.Empty;
        _filas.Clear();
        PanelMapeo.Visibility = Visibility.Collapsed;

        var extension = Path.GetExtension(dialogo.FileName).ToLowerInvariant();

        try
        {
            _filasCrudas = (extension == ".pdf"
                ? AppServices.Importacion.LeerPdf(dialogo.FileName)
                : AppServices.Importacion.LeerXls(dialogo.FileName)) ?? new List<Dictionary<string, string>>();
        }
        catch (Exception ex)
        {
            TxtMensaje.Text = $"No se pudo leer el archivo: {ex.Message}";
            return;
        }

        if (_filasCrudas.Count == 0)
        {
            TxtMensaje.Text = extension == ".pdf"
                ? "No se pudo interpretar una estructura de listado en este PDF. Probá con el archivo XLS de la compañía si lo tenés."
                : "El archivo no tiene filas de datos.";
            return;
        }

        var mapeoAutoDetectado = AppServices.Importacion.AutoDetectarMapeo(_filasCrudas[0].Keys);
        var camposDisponibles = new[] { NoImportar }.Concat(CamposImportacion.Todos).ToArray();

        var filasMapeo = _filasCrudas[0].Keys
            .Select(columna => new ColumnaMapeoFila
            {
                Columna = columna,
                CamposDisponibles = camposDisponibles,
                CampoSeleccionado = mapeoAutoDetectado.TryGetValue(columna, out var campo) ? campo : NoImportar
            })
            .ToList();

        ListaMapeo.ItemsSource = filasMapeo;
        PanelMapeo.Visibility = Visibility.Visible;
    }

    private async void BtnGenerarVistaPrevia_Click(object sender, RoutedEventArgs e)
    {
        var filasMapeo = (IEnumerable<ColumnaMapeoFila>)ListaMapeo.ItemsSource;
        var mapeo = filasMapeo
            .Where(f => f.CampoSeleccionado != NoImportar)
            .ToDictionary(f => f.Columna, f => f.CampoSeleccionado!);

        var preview = await AppServices.Importacion.GenerarVistaPrevia(_filasCrudas, mapeo, AppServices.ProductorActual.Id);

        _filas.Clear();
        foreach (var item in preview)
            _filas.Add(new ItemImportacionFila(item));
    }

    private async void BtnConfirmar_Click(object sender, RoutedEventArgs e)
    {
        if (_filas.Count == 0) return;

        var seleccion = _filas.Select(f => (f.Item, f.Accion));
        var resultado = await AppServices.Importacion.ConfirmarImportacion(AppServices.ProductorActual.Id, seleccion);

        var detalle = string.Join(Environment.NewLine, resultado.Errores);
        MessageBox.Show(
            $"Importados: {resultado.Importados}{Environment.NewLine}Con error: {resultado.ConError}" +
            (resultado.Errores.Count > 0 ? $"{Environment.NewLine}{Environment.NewLine}{detalle}" : ""),
            "Resultado de la importación", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private class ColumnaMapeoFila
    {
        public string Columna { get; set; } = string.Empty;
        public string[] CamposDisponibles { get; set; } = Array.Empty<string>();
        public string? CampoSeleccionado { get; set; }
    }

    public class ItemImportacionFila
    {
        public ItemImportacionFila(ItemImportacion item)
        {
            Item = item;
            Accion = item.TieneError ? AccionImportacion.Omitir
                : item.EsPosibleDuplicado ? AccionImportacion.Actualizar
                : AccionImportacion.Importar;
        }

        public ItemImportacion Item { get; }
        public AccionImportacion Accion { get; set; }
        public AccionImportacion[] AccionesDisponibles { get; } = Enum.GetValues<AccionImportacion>();
    }
}
