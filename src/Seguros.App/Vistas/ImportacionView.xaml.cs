using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Seguros.Data.Importacion;
using Seguros.Domain.Entities;

namespace Seguros.App.Vistas;

public partial class ImportacionView : UserControl
{
    private const string NoImportar = "(no importar)";

    private readonly ObservableCollection<ItemImportacionFila> _filas = new();
    private List<Dictionary<string, string>> _filasCrudas = new();
    private string? _ultimaRutaArchivo;

    public ImportacionView()
    {
        InitializeComponent();
        Grid.ItemsSource = _filas;
        Loaded += async (_, _) => CmbCompaniaPorDefecto.ItemsSource = await AppServices.Companias.ListarCompanias();
    }

    private void BtnElegirArchivo_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog { Filter = "Archivos de listado (*.xlsx;*.csv;*.pdf)|*.xlsx;*.csv;*.pdf" };
        if (dialogo.ShowDialog() != true) return;

        _ultimaRutaArchivo = dialogo.FileName;
        TxtArchivo.Text = Path.GetFileName(dialogo.FileName);
        LeerArchivoSeleccionado();
    }

    private void BtnReleerArchivo_Click(object sender, RoutedEventArgs e)
    {
        if (_ultimaRutaArchivo is null)
        {
            Dialogos.Error("Elegí primero un archivo.", "Nada para releer");
            return;
        }
        LeerArchivoSeleccionado();
    }

    private void LeerArchivoSeleccionado()
    {
        TxtMensaje.Text = string.Empty;
        _filas.Clear();
        PanelMapeo.Visibility = Visibility.Collapsed;

        if (!int.TryParse(TxtFilaEncabezado.Text.Trim(), out var filaEncabezado) || filaEncabezado < 1)
        {
            Dialogos.Error("La fila de encabezado tiene que ser un número mayor o igual a 1.", "Dato inválido");
            return;
        }

        var extension = Path.GetExtension(_ultimaRutaArchivo!).ToLowerInvariant();

        try
        {
            _filasCrudas = extension switch
            {
                ".pdf" => AppServices.Importacion.LeerPdf(_ultimaRutaArchivo!) ?? new List<Dictionary<string, string>>(),
                ".csv" => AppServices.Importacion.LeerCsv(_ultimaRutaArchivo!, filaEncabezado),
                _ => AppServices.Importacion.LeerXls(_ultimaRutaArchivo!, filaEncabezado)
            };
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
                : "El archivo no tiene filas de datos. Revisá el número de fila de encabezado.";
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

        var filasParaPreview = _filasCrudas;

        if (ChkQuedarseConMasReciente.IsChecked == true)
        {
            var columnaClave = mapeo.FirstOrDefault(kv => kv.Value == CamposImportacion.NumeroPoliza).Key;
            var columnaFecha = mapeo.FirstOrDefault(kv => kv.Value == CamposImportacion.VigenciaDesde).Key;

            if (columnaClave is null || columnaFecha is null)
            {
                Dialogos.Error("Para quedarte con la fila más reciente por póliza, mapeá primero las columnas de Número de póliza y Vigencia desde.", "Falta mapeo");
                return;
            }

            filasParaPreview = AppServices.Importacion.DeduplicarPorMasReciente(_filasCrudas, columnaClave, columnaFecha);
        }

        var tieneColumnaCompania = mapeo.Values.Contains(CamposImportacion.CompaniaNombre);
        int? companiaIdPorDefecto = null;

        if (!tieneColumnaCompania)
        {
            if (CmbCompaniaPorDefecto.SelectedItem is not Compania compania)
            {
                Dialogos.Error("Este archivo no trae una columna de compañía: elegí una compañía para aplicarla a todas las filas.", "Falta la compañía");
                return;
            }
            companiaIdPorDefecto = compania.Id;
        }

        var preview = await AppServices.Importacion.GenerarVistaPrevia(filasParaPreview, mapeo, AppServices.ProductorActual.Id, companiaIdPorDefecto);

        _filas.Clear();
        foreach (var item in preview)
            _filas.Add(new ItemImportacionFila(item));
    }

    private async void BtnConfirmar_Click(object sender, RoutedEventArgs e)
    {
        if (_filas.Count == 0)
        {
            Dialogos.Error("No hay filas para importar. Elegí un archivo y generá la vista previa primero.", "Nada para importar");
            return;
        }

        var aImportar = _filas.Count(f => f.Accion != AccionImportacion.Omitir);
        if (!Dialogos.Confirmar($"¿Confirmar la importación de {aImportar} fila(s)? Se van a crear o actualizar asegurados y pólizas en el sistema.", "Confirmar importación"))
            return;

        var seleccion = _filas.Select(f => (f.Item, f.Accion));
        var resultado = await AppServices.Importacion.ConfirmarImportacion(AppServices.ProductorActual.Id, seleccion);

        var detalle = string.Join(Environment.NewLine, resultado.Errores);
        Dialogos.Info(
            $"Importados: {resultado.Importados}{Environment.NewLine}Con error: {resultado.ConError}" +
            (resultado.Errores.Count > 0 ? $"{Environment.NewLine}{Environment.NewLine}{detalle}" : ""),
            "Resultado de la importación");
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
