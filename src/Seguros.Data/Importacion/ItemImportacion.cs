namespace Seguros.Data.Importacion;

/// <summary>Campos del sistema a los que se puede mapear una columna del archivo importado.</summary>
public static class CamposImportacion
{
    public const string TipoDocumento = "TipoDocumento";
    public const string NroDocumento = "NroDocumento";
    public const string NombreAsegurado = "NombreAsegurado";
    public const string Telefono = "Telefono";
    public const string CompaniaNombre = "CompaniaNombre";
    public const string Ramo = "Ramo";
    public const string NumeroPoliza = "NumeroPoliza";
    public const string VigenciaDesde = "VigenciaDesde";
    public const string VigenciaHasta = "VigenciaHasta";
    public const string Prima = "Prima";
    public const string Estado = "Estado";

    public static readonly string[] Todos =
    {
        TipoDocumento, NroDocumento, NombreAsegurado, Telefono, CompaniaNombre, Ramo,
        NumeroPoliza, VigenciaDesde, VigenciaHasta, Prima, Estado
    };
}

public enum AccionImportacion
{
    Importar,
    Omitir,
    Actualizar
}

/// <summary>Una fila del archivo ya mapeada a campos del sistema, lista para revisión en la vista previa.</summary>
public class ItemImportacion
{
    public int NumeroFila { get; set; }

    /// <summary>El documento es opcional: no todas las fuentes lo proveen (specs/asegurados).</summary>
    public string? TipoDocumentoTexto { get; set; }
    public string? NroDocumento { get; set; }

    public string? NombreAsegurado { get; set; }
    public string? Telefono { get; set; }
    public string? CompaniaNombre { get; set; }
    public string? RamoTexto { get; set; }
    public string? NumeroPoliza { get; set; }
    public string? VigenciaDesdeTexto { get; set; }
    public string? VigenciaHastaTexto { get; set; }
    public string? PrimaTexto { get; set; }

    /// <summary>Estado de la póliza según el archivo (ej. "Vigente", "Anulada"). Si no se mapea, se asume vigente.</summary>
    public string? EstadoTexto { get; set; }

    /// <summary>Ya existe un asegurado con este documento (o el mismo nombre, si no hay documento) o una póliza con este número.</summary>
    public bool EsPosibleDuplicado { get; set; }

    /// <summary>Motivo por el que la fila no se puede importar tal cual (dato faltante o inválido).</summary>
    public string? ErrorDeteccion { get; set; }

    public bool TieneError => !string.IsNullOrEmpty(ErrorDeteccion);
}

public class ResultadoImportacion
{
    public int Importados { get; set; }
    public int ConError { get; set; }
    public List<string> Errores { get; } = new();
}
