using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using UglyToad.PdfPig;

namespace Seguros.Data.Importacion;

/// <summary>
/// Lee archivos XLS/CSV/PDF entregados por las compañías como listados tabulares
/// (specs/importacion-datos). XLS/CSV son las fuentes principales y confiables;
/// PDF se trata como "mejor esfuerzo" (ver design.md - Decisión 5).
/// </summary>
public static class LectorArchivos
{
    /// <summary>
    /// <paramref name="filaEncabezado"/> es el número de fila **tal como se ve en Excel**
    /// (1-based, contando también filas en blanco) - algunos exports reales traen filas de
    /// título antes del encabezado real (ej. "CARTERA VIGENTE AL ...", "PAS: 6380").
    /// </summary>
    public static List<Dictionary<string, string>> LeerXls(string rutaArchivo, int filaEncabezado = 1)
    {
        using var workbook = new XLWorkbook(rutaArchivo);
        var hoja = workbook.Worksheets.First();
        var rangoUsado = hoja.RangeUsed();
        if (rangoUsado is null)
            throw new InvalidOperationException("El archivo Excel no tiene filas.");

        var primeraFila = rangoUsado.FirstRow().RowNumber();
        var ultimaFila = rangoUsado.LastRow().RowNumber();
        if (filaEncabezado < primeraFila || filaEncabezado > ultimaFila)
            throw new InvalidOperationException(
                $"La fila de encabezado indicada ({filaEncabezado}) no existe en el archivo (tiene datos entre las filas {primeraFila} y {ultimaFila}).");

        var primeraColumna = rangoUsado.FirstColumn().ColumnNumber();
        var ultimaColumna = rangoUsado.LastColumn().ColumnNumber();
        var filaEncabezados = hoja.Row(filaEncabezado);
        var encabezados = filaEncabezados.Cells(primeraColumna, ultimaColumna)
            .Select(c => c.GetString().Trim())
            .ToList();

        var filas = new List<Dictionary<string, string>>();
        for (var numeroFila = filaEncabezado + 1; numeroFila <= ultimaFila; numeroFila++)
        {
            var filaHoja = hoja.Row(numeroFila);
            var valores = new Dictionary<string, string>();
            for (var i = 0; i < encabezados.Count; i++)
                valores[encabezados[i]] = filaHoja.Cell(primeraColumna + i).GetString().Trim();

            if (valores.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
                filas.Add(valores);
        }

        return filas;
    }

    /// <summary>Lee un CSV (o similar delimitado por ";" o ","), detectando el delimitador automáticamente.</summary>
    public static List<Dictionary<string, string>> LeerCsv(string rutaArchivo, int filaEncabezado = 1)
    {
        var lineas = File.ReadAllLines(rutaArchivo);
        if (lineas.Length == 0)
            throw new InvalidOperationException("El archivo no tiene filas.");
        if (filaEncabezado < 1 || filaEncabezado > lineas.Length)
            throw new InvalidOperationException($"La fila de encabezado indicada ({filaEncabezado}) no existe en el archivo.");

        var indiceEncabezado = filaEncabezado - 1;
        var lineaEncabezado = lineas[indiceEncabezado];
        var delimitador = DetectarDelimitador(lineaEncabezado);
        var encabezados = DividirLineaCsv(lineaEncabezado, delimitador).Select(h => h.Trim()).ToList();

        var filas = new List<Dictionary<string, string>>();
        for (var i = indiceEncabezado + 1; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i])) continue;

            var valoresLinea = DividirLineaCsv(lineas[i], delimitador);
            var valores = new Dictionary<string, string>();
            for (var c = 0; c < encabezados.Count; c++)
                valores[encabezados[c]] = c < valoresLinea.Count ? valoresLinea[c].Trim() : string.Empty;
            filas.Add(valores);
        }

        return filas;
    }

    private static char DetectarDelimitador(string lineaEncabezado)
    {
        var puntoYComa = lineaEncabezado.Count(c => c == ';');
        var coma = lineaEncabezado.Count(c => c == ',');
        return puntoYComa >= coma ? ';' : ',';
    }

    private static List<string> DividirLineaCsv(string linea, char delimitador)
    {
        var campos = new List<string>();
        var actual = new StringBuilder();
        var dentroDeComillas = false;

        for (var i = 0; i < linea.Length; i++)
        {
            var c = linea[i];
            if (c == '"')
            {
                if (dentroDeComillas && i + 1 < linea.Length && linea[i + 1] == '"') { actual.Append('"'); i++; }
                else dentroDeComillas = !dentroDeComillas;
            }
            else if (c == delimitador && !dentroDeComillas)
            {
                campos.Add(actual.ToString());
                actual.Clear();
            }
            else actual.Append(c);
        }
        campos.Add(actual.ToString());
        return campos;
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
