using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Exceptions;

namespace Seguros.Data.Services;

/// <summary>Implementa specs/facturacion-comisiones.</summary>
public class ComisionService
{
    private readonly SegurosDbContext _db;

    public ComisionService(SegurosDbContext db) => _db = db;

    public async Task ConfigurarComision(int companiaId, int ramoId, decimal porcentaje)
    {
        var companiaRamo = await _db.CompaniaRamos
            .FirstOrDefaultAsync(cr => cr.CompaniaId == companiaId && cr.RamoId == ramoId);

        if (companiaRamo is null)
        {
            companiaRamo = new CompaniaRamo { CompaniaId = companiaId, RamoId = ramoId };
            _db.CompaniaRamos.Add(companiaRamo);
        }

        companiaRamo.PorcentajeComision = porcentaje;
        await _db.SaveChangesAsync();
    }

    /// <summary>Comisión calculada para una póliza, o null si no hay porcentaje configurado para su compañía/ramo.</summary>
    public async Task<decimal?> CalcularComision(int polizaId)
    {
        var poliza = await _db.Polizas.FindAsync(polizaId)
            ?? throw new ReglaDeNegocioException("Póliza no encontrada.");

        var companiaRamo = await _db.CompaniaRamos
            .FirstOrDefaultAsync(cr => cr.CompaniaId == poliza.CompaniaId && cr.RamoId == poliza.RamoId);

        if (companiaRamo?.PorcentajeComision is null) return null;

        return Math.Round(poliza.Prima * companiaRamo.PorcentajeComision.Value / 100m, 2);
    }

    public async Task<Liquidacion> GenerarLiquidacion(int productorId, DateOnly periodoDesde, DateOnly periodoHasta)
    {
        var polizasDelPeriodo = await _db.Polizas
            .Where(p => p.ProductorId == productorId
                        && p.VigenciaDesde >= periodoDesde
                        && p.VigenciaDesde <= periodoHasta)
            .ToListAsync();

        var liquidacion = new Liquidacion
        {
            ProductorId = productorId,
            PeriodoDesde = periodoDesde,
            PeriodoHasta = periodoHasta,
            FechaGeneracion = DateTime.Now
        };

        decimal total = 0;
        foreach (var poliza in polizasDelPeriodo)
        {
            var comision = await CalcularComision(poliza.Id);
            if (comision is null) continue;

            liquidacion.Detalles.Add(new LiquidacionDetalle
            {
                PolizaId = poliza.Id,
                MontoComision = comision.Value
            });
            total += comision.Value;
        }

        liquidacion.MontoTotal = total;
        _db.Liquidaciones.Add(liquidacion);
        await _db.SaveChangesAsync();
        return liquidacion;
    }

    public async Task<List<Liquidacion>> HistorialLiquidaciones(int productorId) =>
        await _db.Liquidaciones
            .Where(l => l.ProductorId == productorId)
            .OrderByDescending(l => l.FechaGeneracion)
            .ToListAsync();
}
