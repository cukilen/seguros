using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.Data.Services;

/// <summary>Implementa specs/endosos.</summary>
public class EndosoService
{
    private readonly SegurosDbContext _db;

    public EndosoService(SegurosDbContext db) => _db = db;

    public async Task<Endoso> AltaEndoso(int polizaId, DateOnly fecha, string tipo, string detalle)
    {
        var poliza = await _db.Polizas.FindAsync(polizaId)
            ?? throw new ReglaDeNegocioException("Póliza no encontrada.");
        if (poliza.Estado == EstadoPoliza.Anulada)
            throw new ReglaDeNegocioException("No se puede registrar un endoso sobre una póliza anulada.");

        var endoso = new Endoso
        {
            PolizaId = polizaId,
            Fecha = fecha,
            Tipo = tipo,
            Detalle = detalle,
            Anulado = false
        };

        _db.Endosos.Add(endoso);
        await _db.SaveChangesAsync();
        return endoso;
    }

    public async Task<List<Endoso>> HistorialEndosos(int polizaId) =>
        await _db.Endosos
            .Where(e => e.PolizaId == polizaId)
            .OrderBy(e => e.Fecha)
            .ToListAsync();

    public async Task AnularEndoso(int endosoId, string motivo)
    {
        var endoso = await _db.Endosos.FindAsync(endosoId)
            ?? throw new ReglaDeNegocioException("Endoso no encontrado.");
        endoso.Anulado = true;
        endoso.MotivoAnulacion = motivo;
        await _db.SaveChangesAsync();
    }
}
