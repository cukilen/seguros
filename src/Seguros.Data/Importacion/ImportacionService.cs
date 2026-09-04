using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;

namespace Seguros.Data.Importacion;

/// <summary>Implementa specs/importacion-datos.</summary>
public class ImportacionService
{
    private readonly SegurosDbContext _db;

    public ImportacionService(SegurosDbContext db) => _db = db;

    public List<Dictionary<string, string>> LeerXls(string rutaArchivo) => LectorArchivos.LeerXls(rutaArchivo);

    /// <summary>Null si el PDF no tiene una estructura tabular reconocible (specs/importacion-datos - PDF sin datos reconocibles).</summary>
    public List<Dictionary<string, string>>? LeerPdf(string rutaArchivo) => LectorArchivos.LeerPdf(rutaArchivo);

    public Dictionary<string, string> AutoDetectarMapeo(IEnumerable<string> encabezados) => MapeoColumnas.AutoDetectar(encabezados);

    /// <summary>
    /// Aplica el mapeo de columnas a cada fila cruda y marca posibles duplicados y errores,
    /// para que el productor los revise antes de confirmar la importación.
    /// </summary>
    public async Task<List<ItemImportacion>> GenerarVistaPrevia(
        List<Dictionary<string, string>> filasCrudas,
        Dictionary<string, string> mapeoColumnaACampo,
        int productorId)
    {
        var items = new List<ItemImportacion>();
        var numero = 0;

        foreach (var fila in filasCrudas)
        {
            numero++;
            var item = new ItemImportacion { NumeroFila = numero };

            foreach (var (columna, campo) in mapeoColumnaACampo)
            {
                if (!fila.TryGetValue(columna, out var valor)) continue;

                switch (campo)
                {
                    case CamposImportacion.Documento: item.Documento = valor; break;
                    case CamposImportacion.NombreAsegurado: item.NombreAsegurado = valor; break;
                    case CamposImportacion.Telefono: item.Telefono = valor; break;
                    case CamposImportacion.CompaniaNombre: item.CompaniaNombre = valor; break;
                    case CamposImportacion.Ramo: item.RamoTexto = valor; break;
                    case CamposImportacion.NumeroPoliza: item.NumeroPoliza = valor; break;
                    case CamposImportacion.VigenciaDesde: item.VigenciaDesdeTexto = valor; break;
                    case CamposImportacion.VigenciaHasta: item.VigenciaHastaTexto = valor; break;
                    case CamposImportacion.Prima: item.PrimaTexto = valor; break;
                }
            }

            item.ErrorDeteccion = ValidarCamposMinimos(item);

            if (!item.TieneError)
            {
                var documentoDuplicado = await _db.Asegurados
                    .AnyAsync(a => a.ProductorId == productorId && a.Documento == item.Documento);
                var polizaDuplicada = await _db.Polizas
                    .AnyAsync(p => p.Numero == item.NumeroPoliza);
                item.EsPosibleDuplicado = documentoDuplicado || polizaDuplicada;
            }

            items.Add(item);
        }

        return items;
    }

    private static string? ValidarCamposMinimos(ItemImportacion item)
    {
        if (string.IsNullOrWhiteSpace(item.Documento)) return "Falta el documento del asegurado.";
        if (string.IsNullOrWhiteSpace(item.NombreAsegurado)) return "Falta el nombre del asegurado.";
        if (string.IsNullOrWhiteSpace(item.CompaniaNombre)) return "Falta el nombre de la compañía.";
        if (string.IsNullOrWhiteSpace(item.NumeroPoliza)) return "Falta el número de póliza.";
        if (!TryParseRamo(item.RamoTexto, out _)) return $"Ramo no reconocido: '{item.RamoTexto}'.";
        if (!DateOnly.TryParse(item.VigenciaDesdeTexto, CultureInfo.GetCultureInfo("es-AR"), DateTimeStyles.None, out _))
            return $"Fecha de inicio de vigencia inválida: '{item.VigenciaDesdeTexto}'.";
        if (!DateOnly.TryParse(item.VigenciaHastaTexto, CultureInfo.GetCultureInfo("es-AR"), DateTimeStyles.None, out _))
            return $"Fecha de fin de vigencia inválida: '{item.VigenciaHastaTexto}'.";
        if (!decimal.TryParse(item.PrimaTexto, NumberStyles.Number, CultureInfo.GetCultureInfo("es-AR"), out _))
            return $"Prima inválida: '{item.PrimaTexto}'.";

        return null;
    }

    private static readonly Dictionary<string, Ramo> AliasRamo = new()
    {
        ["vida"] = Ramo.Vida,
        ["incendio"] = Ramo.Incendio,
        ["rc"] = Ramo.ResponsabilidadCivil,
        ["responsabilidadcivil"] = Ramo.ResponsabilidadCivil,
        ["responsabilidad civil"] = Ramo.ResponsabilidadCivil,
        ["auto"] = Ramo.Autos,
        ["autos"] = Ramo.Autos,
        ["combinadofamiliar"] = Ramo.CombinadoFamiliar,
        ["combinado familiar"] = Ramo.CombinadoFamiliar,
        ["accidentespersonales"] = Ramo.AccidentesPersonales,
        ["accidentes personales"] = Ramo.AccidentesPersonales,
        ["ap"] = Ramo.AccidentesPersonales,
    };

    private static bool TryParseRamo(string? texto, out Ramo ramo)
    {
        ramo = default;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        return AliasRamo.TryGetValue(texto.Trim().ToLowerInvariant(), out ramo);
    }

    /// <summary>
    /// Importa los items seleccionados con acción Importar/Actualizar (Omitir se ignora).
    /// Nunca lanza por una fila individual: cada error queda reflejado en el resultado (specs/importacion-datos).
    /// </summary>
    public async Task<ResultadoImportacion> ConfirmarImportacion(
        int productorId, IEnumerable<(ItemImportacion item, AccionImportacion accion)> seleccion)
    {
        var resultado = new ResultadoImportacion();

        foreach (var (item, accion) in seleccion)
        {
            if (accion == AccionImportacion.Omitir) continue;

            try
            {
                if (item.TieneError)
                    throw new InvalidOperationException(item.ErrorDeteccion);

                await ImportarFila(productorId, item);
                resultado.Importados++;
            }
            catch (Exception ex)
            {
                resultado.ConError++;
                resultado.Errores.Add($"Fila {item.NumeroFila}: {ex.Message}");
            }
        }

        return resultado;
    }

    private async Task ImportarFila(int productorId, ItemImportacion item)
    {
        var compania = await _db.Companias
            .FirstOrDefaultAsync(c => c.Nombre.ToLower() == item.CompaniaNombre!.ToLower())
            ?? throw new InvalidOperationException($"Compañía '{item.CompaniaNombre}' no está registrada.");

        TryParseRamo(item.RamoTexto, out var ramo);
        var cultura = CultureInfo.GetCultureInfo("es-AR");
        var vigenciaDesde = DateOnly.Parse(item.VigenciaDesdeTexto!, cultura);
        var vigenciaHasta = DateOnly.Parse(item.VigenciaHastaTexto!, cultura);
        var prima = decimal.Parse(item.PrimaTexto!, NumberStyles.Number, cultura);

        var asegurado = await _db.Asegurados
            .FirstOrDefaultAsync(a => a.ProductorId == productorId && a.Documento == item.Documento);

        if (asegurado is null)
        {
            asegurado = new Asegurado
            {
                ProductorId = productorId,
                Documento = item.Documento!,
                Nombre = item.NombreAsegurado!,
                Telefono = item.Telefono
            };
            _db.Asegurados.Add(asegurado);
            await _db.SaveChangesAsync();
        }

        var poliza = await _db.Polizas.FirstOrDefaultAsync(p => p.Numero == item.NumeroPoliza && p.CompaniaId == compania.Id);
        if (poliza is null)
        {
            poliza = new Poliza
            {
                ProductorId = productorId,
                AseguradoId = asegurado.Id,
                CompaniaId = compania.Id,
                Ramo = ramo,
                Numero = item.NumeroPoliza!,
                VigenciaDesde = vigenciaDesde,
                VigenciaHasta = vigenciaHasta,
                Prima = prima,
                Estado = EstadoPoliza.Vigente
            };
            _db.Polizas.Add(poliza);
        }
        else
        {
            poliza.VigenciaDesde = vigenciaDesde;
            poliza.VigenciaHasta = vigenciaHasta;
            poliza.Prima = prima;
        }

        await _db.SaveChangesAsync();
    }
}
