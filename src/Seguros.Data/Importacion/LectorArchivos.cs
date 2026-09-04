using System.Text.RegularExpressions;
using ClosedXML.Excel;
using UglyToad.PdfPig;

namespace Seguros.Data.Importacion;

/// <summary>
/// Lee archivos XLS/PDF entregados por las compañías como listados tabulares
/// (specs/importacion-datos). XLS es la fuente principal y confiable; PDF se
/// trata como "mejor esfuerzo" (ver design.md - Decisión 5).
/// </summary>
public static class LectorArchivos
{
    /// <summary>Primera fila = encabezados, resto = datos. Cada fila es encabezado -> valor.</summary>
    public static List<Dictionary<string, string>> LeerXls(string rutaArchivo)
    {
        using var workbook = new XLWorkbook(rutaArchivo);
        var hoja = workbook.Worksheets.First();
        var filasUsadas = hoja.RowsUsed().ToList();
        if (filasUsadas.Count == 0)
            throw new InvalidOperationException("El archivo Excel no tiene filas.");

        var filaEncabezados = filasUsadas[0];
        var ultimaColumna = filaEncabezados.LastCellUsed()!.Address.ColumnNumber;
        var encabezados = filaEncabezados.Cells(1, ultimaColumna)
            .Select(c => c.GetString().Trim())
            .ToList();

        var filas = new List<Dictionary<string, string>>();
        foreach (var filaHoja in filasUsadas.Skip(1))
        {
            var valores = new Dictionary<string, string>();
            for (var i = 0; i < encabezados.Count; i++)
                valores[encabezados[i]] = filaHoja.Cell(i + 1).GetString().Trim();

            if (valores.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
                filas.Add(valores);
        }

        return filas;
    }

    /// <summary>
    /// Intenta extraer un listado tabular del texto del PDF (columnas separadas por 2+ espacios o tabs).
    /// Devuelve null si no se pudo detectar una estructura de al menos 2 columnas.
    /// </summary>
    public static List<Dictionary<string, string>>? LeerPdf(string rutaArchivo)
    {
        using var documento = PdfDocument.Open(rutaArchivo);

        var lineas = new List<string>();
        foreach (var pagina in documento.GetPages())
        {
            var texto = pagina.Text;
            lineas.AddRange(texto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        return InterpretarLineasTabulares(lineas);
    }

    /// <summary>
    /// Lógica de interpretación de columnas, separada de la lectura del PDF para poder
    /// probarla sin depender de un archivo real. Devuelve null si no se pudo detectar
    /// una estructura de al menos 2 columnas en al menos 2 líneas (encabezado + 1 dato).
    /// </summary>
    internal static List<Dictionary<string, string>>? InterpretarLineasTabulares(IEnumerable<string> lineas)
    {
        var separador = new Regex(@"\s{2,}|\t");
        var filasDivididas = lineas
            .Select(l => separador.Split(l).Where(c => c.Length > 0).ToArray())
            .Where(cols => cols.Length >= 2)
            .ToList();

        if (filasDivididas.Count < 2)
            return null; // no hay suficientes filas con estructura tabular reconocible

        var encabezados = filasDivididas[0];
        var filas = new List<Dictionary<string, string>>();

        foreach (var cols in filasDivididas.Skip(1))
        {
            var valores = new Dictionary<string, string>();
            for (var i = 0; i < encabezados.Length; i++)
                valores[encabezados[i]] = i < cols.Length ? cols[i] : string.Empty;
            filas.Add(valores);
        }

        return filas;
    }
}
