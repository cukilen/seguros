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

    public async Task<Poliza> AltaPoliza(int productorId, int aseguradoId, int companiaId, Ramo ramo,
        string numero, DateOnly vigenciaDesde, DateOnly vigenciaHasta, decimal prima, bool esFlota = false)
    {
        var numeroDuplicado = await _db.Polizas.AnyAsync(p => p.CompaniaId == companiaId && p.Numero == numero);
        if (numeroDuplicado)
            throw new ReglaDeNegocioException($"El número de póliza '{numero}' ya está en uso para esta compañía.");

        var poliza = new Poliza
        {
            ProductorId = productorId,
            AseguradoId = aseguradoId,
            CompaniaId = companiaId,
            Ramo = ramo,
            Numero = numero,
            VigenciaDesde = vigenciaDesde,
            VigenciaHasta = vigenciaHasta,
            Prima = prima,
            EsFlota = esFlota && ramo == Ramo.Autos,
            Estado = EstadoPoliza.Vigente
        };

        _db.Polizas.Add(poliza);
        await _db.SaveChangesAsync();
        return poliza;
    }

    public async Task EditarPoliza(int polizaId, decimal prima, DateOnly vigenciaHasta)
    {
        var poliza = await ObtenerVigenteOFallar(polizaId);
        poliza.Prima = prima;
        poliza.VigenciaHasta = vigenciaHasta;
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
            Ramo = original.Ramo,
            Numero = nuevoNumero,
            VigenciaDesde = nuevaVigenciaDesde,
            VigenciaHasta = nuevaVigenciaHasta,
            Prima = nuevaPrima,
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

    public async Task<List<Poliza>> Consultar(int? aseguradoId = null, int? companiaId = null, Ramo? ramo = null)
    {
        var query = _db.Polizas.Include(p => p.Compania).Include(p => p.Asegurado).AsQueryable();

        if (aseguradoId is not null) query = query.Where(p => p.AseguradoId == aseguradoId);
        if (companiaId is not null) query = query.Where(p => p.CompaniaId == companiaId);
        if (ramo is not null) query = query.Where(p => p.Ramo == ramo);

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
}
