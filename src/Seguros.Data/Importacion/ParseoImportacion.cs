using System.Globalization;

namespace Seguros.Data.Importacion;

/// <summary>
/// Parseo tolerante de fechas y montos: los archivos reales analizados no usan un
/// único formato (tasks.md 5.3/5.4) - algunos traen fechas con año de 2 dígitos,
/// otros montos con coma de miles y punto decimal en vez del formato es-AR.
/// </summary>
public static class ParseoImportacion
{
    private static readonly CultureInfo EsAr = CultureInfo.GetCultureInfo("es-AR");

    public static bool TryParseFecha(string? texto, out DateOnly fecha)
    {
        fecha = default;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        texto = texto.Trim();

        if (DateOnly.TryParse(texto, EsAr, DateTimeStyles.None, out fecha)) return true;
        if (DateOnly.TryParseExact(texto, "dd/MM/yy", EsAr, DateTimeStyles.None, out fecha)) return true;
        if (DateOnly.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha)) return true;

        return false;
    }

    public static bool TryParseMonto(string? texto, out decimal monto)
    {
        monto = default;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        texto = texto.Trim().TrimStart('$').Trim();

        // es-AR: punto de miles, coma decimal (ej. "20621,3")
        if (decimal.TryParse(texto, NumberStyles.Number, EsAr, out monto)) return true;

        // Formato con coma de miles y punto decimal (ej. "199,036.44")
        if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out monto)) return true;

        return false;
    }
}
