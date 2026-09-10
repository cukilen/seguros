using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.Data.Services;

/// <summary>Implementa specs/polizas.</summary>
public class PolizaService
{
    private readonly SegurosDbContext _db;

    public PolizaService(SegurosDbContext db) => _db = db;

    public async Task<Poliza> AltaPoliza(int productorId, int aseguradoId, int companiaId, int ramoId,
        string numero, DateOnly vigenciaDesde, DateOnly vigenciaHasta, decimal prima, bool esFlota = false,
        string? producto = null, decimal? premio = null, string? cobertura = null, string? descripcionRiesgo = null)
    {
        var numeroDuplicado = await _db.Polizas.AnyAsync(p => p.CompaniaId == companiaId && p.Numero == numero);
        if (numeroDuplicado)
            throw new ReglaDeNegocioException($"El número de póliza '{numero}' ya está en uso para esta compañía.");

        var ramo = await _db.Ramos.FindAsync(ramoId)
            ?? throw new ReglaDeNegocioException("Ramo no encontrado.");

        var poliza = new Poliza
        {
            ProductorId = productorId,
            AseguradoId = aseguradoId,
            CompaniaId = companiaId,
            RamoId = ramoId,
            Numero = numero,
            VigenciaDesde = vigenciaDesde,
            VigenciaHasta = vigenciaHasta,
            Prima = prima,
            Producto = producto,
            Premio = premio,
            Cobertura = cobertura,
            DescripcionRiesgo = esFlota ? null : descripcionRiesgo,
            EsFlota = esFlota && EsRamoAutos(ramo),
            Estado = EstadoPoliza.Vigente
        };

        _db.Polizas.Add(poliza);
        await _db.SaveChangesAsync();
        return poliza;
    }

    public async Task EditarPoliza(int polizaId, decimal prima, DateOnly vigenciaHasta,
        string? producto = null, decimal? premio = null, string? cobertura = null, string? descripcionRiesgo = null)
    {
        var poliza = await ObtenerVigenteOFallar(polizaId);
        poliza.Prima = prima;
        poliza.VigenciaHasta = vigenciaHasta;
        poliza.Producto = producto;
        poliza.Premio = premio;
        poliza.Cobertura = cobertura;
        if (!poliza.EsFlota) poliza.DescripcionRiesgo = descripcionRiesgo;
        await _db.SaveChangesAsync();
    }

    /// <summary>Genera una nueva póliza vinculada a la original y marca la original como renovada.</summary>
    public async Task<Poliza> RenovarPoliza(int polizaOriginalId, string nuevoNumero, DateOnly nuevaVigenciaDesde, DateOnly nuevaVigenciaHasta, decimal nuevaPrima)
    {
        var original = await ObtenerVigenteOFallar(polizaOriginalId);

        var numeroDuplicado = await _db.Polizas.AnyAsync(p => p.CompaniaId == original.CompaniaId && p.Numero == nuevoNumero);
        if (numeroDuplicado)
            throw new ReglaDeNegocioException($"El número de póliza '{nuevoNumero}' ya está en uso para esta compañía.");

        var nueva = new Poliza
        {
            ProductorId = original.ProductorId,
            AseguradoId = original.AseguradoId,
            CompaniaId = original.CompaniaId,
            RamoId = original.RamoId,
            Numero = nuevoNumero,
            VigenciaDesde = nuevaVigenciaDesde,
            VigenciaHasta = nuevaVigenciaHasta,
            Prima = nuevaPrima,
            Producto = original.Producto,
            Premio = original.Premio,
            Cobertura = original.Cobertura,
            DescripcionRiesgo = original.DescripcionRiesgo,
            EsFlota = original.EsFlota,
            Estado = EstadoPoliza.Vigente,
            PolizaOrigenId = original.Id
        };

        original.Estado = EstadoPoliza.Renovada;
        original.VencimientoGestionado = true;

        _db.Polizas.Add(nueva);
        await _db.SaveChangesAsync();
        return nueva;
    }

    public async Task AnularPoliza(int polizaId, string motivo)
    {
        var poliza = await ObtenerVigenteOFallar(polizaId);
        poliza.Estado = EstadoPoliza.Anulada;
        poliza.MotivoAnulacion = motivo;
        poliza.VencimientoGestionado = true;
        await _db.SaveChangesAsync();
    }

    /// <summary>Marca una póliza vigente vencida sin gestión como no vigente (distinto de anulada).</summary>
    public async Task MarcarComoNoVigente(int polizaId)
    {
        var poliza = await _db.Polizas.FindAsync(polizaId)
            ?? throw new ReglaDeNegocioException("Póliza no encontrada.");
        if (poliza.Estado != EstadoPoliza.Vigente)
            throw new ReglaDeNegocioException("Solo una póliza vigente puede marcarse como no vigente.");

        poliza.Estado = EstadoPoliza.NoVigente;
        poliza.VencimientoGestionado = true;
        await _db.SaveChangesAsync();
    }

    public async Task<List<Poliza>> Consultar(int? aseguradoId = null, int? companiaId = null, int? ramoId = null)
    {
        var query = _db.Polizas.Include(p => p.Compania).Include(p => p.Asegurado).Include(p => p.Ramo).AsQueryable();

        if (aseguradoId is not null) query = query.Where(p => p.AseguradoId == aseguradoId);
        if (companiaId is not null) query = query.Where(p => p.CompaniaId == companiaId);
        if (ramoId is not null) query = query.Where(p => p.RamoId == ramoId);

        return await query.OrderByDescending(p => p.VigenciaDesde).ToListAsync();
    }

    internal async Task<Poliza> ObtenerVigenteOFallar(int polizaId)
    {
        var poliza = await _db.Polizas.FindAsync(polizaId)
            ?? throw new ReglaDeNegocioException("Póliza no encontrada.");
        if (poliza.Estado == EstadoPoliza.Anulada)
            throw new ReglaDeNegocioException("La póliza está anulada y no admite esta operación.");
        return poliza;
    }

    public static bool EsRamoAutos(Ramo ramo) => ramo.Nombre.Equals("Autos", StringComparison.OrdinalIgnoreCase);
}
