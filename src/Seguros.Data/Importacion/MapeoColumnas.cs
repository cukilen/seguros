using System.Globalization;
using System.Text;

namespace Seguros.Data.Importacion;

/// <summary>Intenta reconocer automáticamente a qué campo del sistema corresponde cada columna del archivo.</summary>
public static class MapeoColumnas
{
    private static readonly Dictionary<string, string[]> Alias = new()
    {
        [CamposImportacion.TipoDocumento] = new[] { "tipodoc", "tipodocumento" },
        [CamposImportacion.NroDocumento] = new[] { "nrodocumento", "nrodoc", "documento", "dni", "cuit" },
        [CamposImportacion.NombreAsegurado] = new[] { "asegurado", "nombreasegurado", "cliente", "nombre" },
        [CamposImportacion.Telefono] = new[] { "telefono", "tel", "celular" },
        [CamposImportacion.CompaniaNombre] = new[] { "compania", "aseguradora", "cia" },
        [CamposImportacion.Ramo] = new[] { "ramo", "seccion" },
        [CamposImportacion.NumeroPoliza] = new[] { "nropoliza", "numeropoliza", "poliza", "nrpoliza" },
        [CamposImportacion.VigenciaDesde] = new[] { "vigenciadesde", "desde", "inicio", "fechadesde" },
        [CamposImportacion.VigenciaHasta] = new[] { "vigenciahasta", "hasta", "vencimiento", "fechahasta" },
        [CamposImportacion.Prima] = new[] { "prima", "importe", "monto" },
        [CamposImportacion.Estado] = new[] { "estado" },
    };

    /// <summary>Devuelve columna del archivo -> campo del sistema, para las columnas que se pudieron reconocer.</summary>
    public static Dictionary<string, string> AutoDetectar(IEnumerable<string> encabezados)
    {
        var mapeo = new Dictionary<string, string>();
        foreach (var encabezado in encabezados)
        {
            var normalizado = Normalizar(encabezado);
            var campo = Alias.FirstOrDefault(kv => kv.Value.Any(alias => normalizado.Contains(alias))).Key;
            if (campo is not null)
                mapeo[encabezado] = campo;
        }
        return mapeo;
    }

    private static string Normalizar(string texto)
    {
        var sinAcentos = string.Concat(texto.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        return sinAcentos.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("°", "").Replace("º", "");
    }
}
