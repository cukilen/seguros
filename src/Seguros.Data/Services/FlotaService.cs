using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.Data.Services;

/// <summary>Implementa specs/flotas.</summary>
public class FlotaService
{
    private readonly SegurosDbContext _db;

    public FlotaService(SegurosDbContext db) => _db = db;

    public async Task MarcarComoFlota(int polizaId)
    {
        var poliza = await _db.Polizas.FindAsync(polizaId)
            ?? throw new ReglaDeNegocioException("Póliza no encontrada.");
        if (poliza.Ramo != Ramo.Autos)
            throw new ReglaDeNegocioException("Sólo una póliza del ramo autos puede marcarse como flota.");

        poliza.EsFlota = true;
        await _db.SaveChangesAsync();
    }

    public async Task<UnidadFlota> AltaUnidad(int polizaId, string patente, string marca, string modelo, string uso)
    {
        var poliza = await _db.Polizas.FindAsync(polizaId)
            ?? throw new ReglaDeNegocioException("Póliza no encontrada.");
        if (!poliza.EsFlota)
            throw new ReglaDeNegocioException("La póliza no está marcada como flota.");

        var duplicada = await _db.UnidadesFlota.AnyAsync(u => u.PolizaId == polizaId && u.Patente == patente && u.Activa);
        if (duplicada)
            throw new ReglaDeNegocioException($"La unidad con patente '{patente}' ya está activa en esta flota.");

        var unidad = new UnidadFlota
        {
            PolizaId = polizaId,
            Patente = patente,
            Marca = marca,
            Modelo = modelo,
            Uso = uso,
            Activa = true
        };

        _db.UnidadesFlota.Add(unidad);
        await _db.SaveChangesAsync();
        return unidad;
    }

    public async Task BajaUnidad(int unidadId, DateOnly fechaBaja)
    {
        var unidad = await _db.UnidadesFlota.FindAsync(unidadId)
            ?? throw new ReglaDeNegocioException("Unidad no encontrada.");
        unidad.Activa = false;
        unidad.FechaBaja = fechaBaja;
        await _db.SaveChangesAsync();
    }

    public async Task<List<UnidadFlota>> ListarUnidades(int polizaId) =>
        await _db.UnidadesFlota.Where(u => u.PolizaId == polizaId).ToListAsync();
}
