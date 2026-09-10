using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Exceptions;

namespace Seguros.Data.Services;

/// <summary>Implementa specs/ramos: catálogo abierto de ramos/secciones.</summary>
public class RamoService
{
    private readonly SegurosDbContext _db;

    public RamoService(SegurosDbContext db) => _db = db;

    public async Task<Ramo> AltaRamo(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre del ramo es obligatorio.");

        var yaExiste = await _db.Ramos.AnyAsync(r => r.Nombre.ToLower() == nombre.Trim().ToLower());
        if (yaExiste)
            throw new ReglaDeNegocioException($"Ya existe un ramo llamado '{nombre}'.");

        var ramo = new Ramo { Nombre = nombre.Trim() };
        _db.Ramos.Add(ramo);
        await _db.SaveChangesAsync();
        return ramo;
    }

    public async Task<List<Ramo>> Listar() =>
        await _db.Ramos.OrderBy(r => r.Nombre).ToListAsync();

    /// <summary>Busca un ramo por nombre exacto (sin distinguir mayúsculas/minúsculas), o null si no existe.</summary>
    public async Task<Ramo?> BuscarPorNombre(string nombre) =>
        await _db.Ramos.FirstOrDefaultAsync(r => r.Nombre.ToLower() == nombre.Trim().ToLower());
}
