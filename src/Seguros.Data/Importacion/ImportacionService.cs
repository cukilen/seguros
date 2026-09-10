using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;

namespace Seguros.Data.Importacion;

/// <summary>Implementa specs/importacion-datos.</summary>
public class ImportacionService
{
    private readonly SegurosDbContext _db;

    public ImportacionService(SegurosDbContext db) => _db = db;

    public List<Dictionary<string, string>> LeerXls(string rutaArchivo, int filaEncabezado = 1) =>
        LectorArchivos.LeerXls(rutaArchivo, filaEncabezado);

    public List<Dictionary<string, string>> LeerCsv(string rutaArchivo, int filaEncabezado = 1) =>
        LectorArchivos.LeerCsv(rutaArchivo, filaEncabezado);

    /// <summary>Null si el PDF no tiene una estructura tabular reconocible (specs/importacion-datos - PDF sin datos reconocibles).</summary>
    public List<Dictionary<string, string>>? LeerPdf(string rutaArchivo) => LectorArchivos.LeerPdf(rutaArchivo);

    public Dictionary<string, string> AutoDetectarMapeo(IEnumerable<string> encabezados) => MapeoColumnas.AutoDetectar(encabezados);

    /// <summary>
    /// Para archivos de historial (varias filas por póliza): se queda con una sola fila por
    /// cada valor de <paramref name="columnaClave"/>, la de fecha más reciente en
    /// <paramref name="columnaFecha"/>. Filas sin fecha parseable quedan al final.
    /// </summary>
    public List<Dictionary<string, string>> DeduplicarPorMasReciente(
        List<Dictionary<string, string>> filas, string columnaClave, string columnaFecha)
    {
        return filas
            .GroupBy(f => f.GetValueOrDefault(columnaClave, string.Empty))
            .Select(grupo => grupo
                .OrderByDescending(f => ParseoImportacion.TryParseFecha(f.GetValueOrDefault(columnaFecha), out var fecha) ? fecha : DateOnly.MinValue)
                .First())
            .ToList();
    }

    /// <summary>
    /// Aplica el mapeo de columnas a cada fila cruda y marca posibles duplicados y errores,
    /// para que el productor los revise antes de confirmar la importación.
    /// </summary>
    /// <param name="companiaIdPorDefecto">
    /// Compañía a usar en filas donde el mapeo no incluye una columna de compañía
    /// (specs/importacion-datos - Determinación de la compañía al importar).
    /// </param>
    public async Task<List<ItemImportacion>> GenerarVistaPrevia(
        List<Dictionary<string, string>> filasCrudas,
        Dictionary<string, string> mapeoColumnaACampo,
        int productorId,
        int? companiaIdPorDefecto = null)
    {
        string? nombreCompaniaPorDefecto = null;
        if (companiaIdPorDefecto is not null)
        {
            var compania = await _db.Companias.FindAsync(companiaIdPorDefecto.Value);
            nombreCompaniaPorDefecto = compania?.Nombre;
        }

        var items = new List<ItemImportacion>();
        var numero = 0;

        foreach (var fila in filasCrudas)
        {
            numero++;
            var item = new ItemImportacion { NumeroFila = numero, CompaniaNombre = nombreCompaniaPorDefecto };

            foreach (var (columna, campo) in mapeoColumnaACampo)
            {
                if (!fila.TryGetValue(columna, out var valor)) continue;

                switch (campo)
                {
                    case CamposImportacion.TipoDocumento: item.TipoDocumentoTexto = valor; break;
                    case CamposImportacion.NroDocumento: item.NroDocumento = valor; break;
                    case CamposImportacion.NombreAsegurado: item.NombreAsegurado = valor; break;
                    case CamposImportacion.Telefono: item.Telefono = valor; break;
                    case CamposImportacion.CompaniaNombre: item.CompaniaNombre = valor; break;
                    case CamposImportacion.Ramo: item.RamoTexto = valor; break;
                    case CamposImportacion.NumeroPoliza: item.NumeroPoliza = valor; break;
                    case CamposImportacion.VigenciaDesde: item.VigenciaDesdeTexto = valor; break;
                    case CamposImportacion.VigenciaHasta: item.VigenciaHastaTexto = valor; break;
                    case CamposImportacion.Prima: item.PrimaTexto = valor; break;
                    case CamposImportacion.Estado: item.EstadoTexto = valor; break;
                }
            }

            item.ErrorDeteccion = ValidarCamposMinimos(item);

            if (!item.TieneError)
                item.EsPosibleDuplicado = await EsPosibleDuplicado(productorId, item);

            items.Add(item);
        }

        return items;
    }

    private async Task<bool> EsPosibleDuplicado(int productorId, ItemImportacion item)
    {
        var polizaDuplicada = await _db.Polizas.AnyAsync(p => p.Numero == item.NumeroPoliza);
        if (polizaDuplicada) return true;

        if (!string.IsNullOrWhiteSpace(item.NroDocumento))
        {
            var tipoDoc = ParsearTipoDocumento(item.TipoDocumentoTexto) ?? TipoDocumento.Dni;
            return await _db.Asegurados.AnyAsync(a =>
                a.ProductorId == productorId && a.TipoDocumento == tipoDoc && a.NroDocumento == item.NroDocumento);
        }

        // Sin documento: la única señal posible es el nombre (limitación conocida, ver design.md).
        return await _db.Asegurados.AnyAsync(a =>
            a.ProductorId == productorId && a.Nombre.ToLower() == item.NombreAsegurado!.ToLower());
    }

    private static string? ValidarCamposMinimos(ItemImportacion item)
    {
        if (string.IsNullOrWhiteSpace(item.NombreAsegurado)) return "Falta el nombre del asegurado.";
        if (string.IsNullOrWhiteSpace(item.CompaniaNombre)) return "Falta el nombre de la compañía.";
        if (string.IsNullOrWhiteSpace(item.NumeroPoliza)) return "Falta el número de póliza.";
        if (string.IsNullOrWhiteSpace(item.RamoTexto)) return "Falta el ramo.";
        if (!ParseoImportacion.TryParseFecha(item.VigenciaDesdeTexto, out _))
            return $"Fecha de inicio de vigencia inválida: '{item.VigenciaDesdeTexto}'.";
        if (!ParseoImportacion.TryParseFecha(item.VigenciaHastaTexto, out _))
            return $"Fecha de fin de vigencia inválida: '{item.VigenciaHastaTexto}'.";
        if (item.PrimaTexto is not null && !ParseoImportacion.TryParseMonto(item.PrimaTexto, out _))
            return $"Prima inválida: '{item.PrimaTexto}'.";

        return null;
    }

    private static readonly Dictionary<string, TipoDocumento> AliasTipoDocumento = new()
    {
        ["dni"] = TipoDocumento.Dni,
        ["cuit"] = TipoDocumento.Cuit,
        ["le"] = TipoDocumento.Le,
    };

    /// <summary>Null si no hay texto o no se reconoce (se usa Dni como default al importar).</summary>
    private static TipoDocumento? ParsearTipoDocumento(string? texto) =>
        !string.IsNullOrWhiteSpace(texto) && AliasTipoDocumento.TryGetValue(texto.Trim().ToLowerInvariant(), out var tipo)
            ? tipo
            : null;

    /// <summary>
    /// Interpreta el estado de la póliza según el texto del archivo. Si no hay columna de
    /// estado mapeada, o el texto no se reconoce, se asume Vigente (comportamiento anterior).
    /// </summary>
    private static EstadoPoliza ParsearEstado(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return EstadoPoliza.Vigente;

        var normalizado = texto.Trim().ToLowerInvariant();
        if (normalizado.Contains("anula")) return EstadoPoliza.Anulada;
        if (normalizado.Contains("no vigente") || normalizado.Contains("novigente")) return EstadoPoliza.NoVigente;
        if (normalizado.Contains("renovada")) return EstadoPoliza.Renovada;
        return EstadoPoliza.Vigente;
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

    private async Task<Ramo> ObtenerOCrearRamo(string nombreRamo)
    {
        var ramo = await _db.Ramos.FirstOrDefaultAsync(r => r.Nombre.ToLower() == nombreRamo.Trim().ToLower());
        if (ramo is not null) return ramo;

        ramo = new Ramo { Nombre = nombreRamo.Trim() };
        _db.Ramos.Add(ramo);
        await _db.SaveChangesAsync();
        return ramo;
    }

    private async Task ImportarFila(int productorId, ItemImportacion item)
    {
        var compania = await _db.Companias
            .FirstOrDefaultAsync(c => c.Nombre.ToLower() == item.CompaniaNombre!.ToLower())
            ?? throw new InvalidOperationException($"Compañía '{item.CompaniaNombre}' no está registrada.");

        var ramo = await ObtenerOCrearRamo(item.RamoTexto!);
        ParseoImportacion.TryParseFecha(item.VigenciaDesdeTexto, out var vigenciaDesde);
        ParseoImportacion.TryParseFecha(item.VigenciaHastaTexto, out var vigenciaHasta);
        var prima = 0m;
        if (item.PrimaTexto is not null) ParseoImportacion.TryParseMonto(item.PrimaTexto, out prima);

        Asegurado? asegurado = null;
        var tieneDocumento = !string.IsNullOrWhiteSpace(item.NroDocumento);
        var tipoDocumento = ParsearTipoDocumento(item.TipoDocumentoTexto) ?? TipoDocumento.Dni;

        if (tieneDocumento)
        {
            asegurado = await _db.Asegurados.FirstOrDefaultAsync(a =>
                a.ProductorId == productorId && a.TipoDocumento == tipoDocumento && a.NroDocumento == item.NroDocumento);
        }
        else
        {
            asegurado = await _db.Asegurados.FirstOrDefaultAsync(a =>
                a.ProductorId == productorId && a.Nombre.ToLower() == item.NombreAsegurado!.ToLower());
        }

        if (asegurado is null)
        {
            asegurado = new Asegurado
            {
                ProductorId = productorId,
                TipoDocumento = tieneDocumento ? tipoDocumento : null,
                NroDocumento = tieneDocumento ? item.NroDocumento : null,
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
                RamoId = ramo.Id,
                Numero = item.NumeroPoliza!,
                VigenciaDesde = vigenciaDesde,
                VigenciaHasta = vigenciaHasta,
                Prima = prima,
                Estado = ParsearEstado(item.EstadoTexto)
            };
            _db.Polizas.Add(poliza);
        }
        else
        {
            poliza.VigenciaDesde = vigenciaDesde;
            poliza.VigenciaHasta = vigenciaHasta;
            poliza.Prima = prima;
            poliza.Estado = ParsearEstado(item.EstadoTexto);
        }

        await _db.SaveChangesAsync();
    }
}
