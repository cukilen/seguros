using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.Data.Services;

/// <summary>Implementa specs/vencimientos: alertas internas, sin notificaciones externas.</summary>
public class VencimientoService
{
    private readonly SegurosDbContext _db;

    public VencimientoService(SegurosDbContext db) => _db = db;

    /// <summary>Pólizas vigentes cuyo vencimiento cae dentro de la ventana de días indicada (por defecto 30).</summary>
    public async Task<List<Poliza>> ListarProximasAVencer(int diasVentana = 30, int? productorId = null, int? companiaId = null, int? ramoId = null)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var limite = hoy.AddDays(diasVentana);

        var query = _db.Polizas.Include(p => p.Compania).Include(p => p.Asegurado).Include(p => p.Ramo)
            .Where(p => p.Estado == EstadoPoliza.Vigente
                        && !p.VencimientoGestionado
                        && p.VigenciaHasta >= hoy
                        && p.VigenciaHasta <= limite);

        if (productorId is not null) query = query.Where(p => p.ProductorId == productorId);
        if (companiaId is not null) query = query.Where(p => p.CompaniaId == companiaId);
        if (ramoId is not null) query = query.Where(p => p.RamoId == ramoId);

        return await query.OrderBy(p => p.VigenciaHasta).ToListAsync();
    }

    public async Task<List<Poliza>> ListarVencidas(int? productorId = null, int? companiaId = null, int? ramoId = null)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var query = _db.Polizas.Include(p => p.Compania).Include(p => p.Asegurado).Include(p => p.Ramo)
            .Where(p => p.Estado == EstadoPoliza.Vigente
                        && !p.VencimientoGestionado
                        && p.VigenciaHasta < hoy);

        if (productorId is not null) query = query.Where(p => p.ProductorId == productorId);
        if (companiaId is not null) query = query.Where(p => p.CompaniaId == companiaId);
        if (ramoId is not null) query = query.Where(p => p.RamoId == ramoId);

        return await query.OrderBy(p => p.VigenciaHasta).ToListAsync();
    }

    public async Task MarcarComoGestionado(int polizaId)
    {
        var poliza = await _db.Polizas.FindAsync(polizaId)
            ?? throw new ReglaDeNegocioException("Póliza no encontrada.");
        poliza.VencimientoGestionado = true;
        await _db.SaveChangesAsync();
    }
}
