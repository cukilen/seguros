using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.Data.Services;

/// <summary>Implementa specs/companias.</summary>
public class CompaniaService
{
    private readonly SegurosDbContext _db;

    public CompaniaService(SegurosDbContext db) => _db = db;

    public async Task<Compania> AltaCompania(string nombre, IEnumerable<Ramo> ramosOperados, string? telefono = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la compañía es obligatorio.");

        var compania = new Compania
        {
            Nombre = nombre,
            Telefono = telefono,
            Email = email,
            Activa = true
        };

        foreach (var ramo in ramosOperados.Distinct())
            compania.RamosOperados.Add(new CompaniaRamo { Ramo = ramo });

        _db.Companias.Add(compania);
        await _db.SaveChangesAsync();
        return compania;
    }

    public async Task EditarDatosContacto(int companiaId, string? telefono, string? email)
    {
        var compania = await _db.Companias.FindAsync(companiaId)
            ?? throw new ReglaDeNegocioException("Compañía no encontrada.");
        compania.Telefono = telefono;
        compania.Email = email;
        await _db.SaveChangesAsync();
    }

    public async Task<List<Compania>> ListarCompanias() =>
        await _db.Companias.Include(c => c.RamosOperados).ToListAsync();

    public async Task InactivarCompania(int companiaId)
    {
        var compania = await _db.Companias.FindAsync(companiaId)
            ?? throw new ReglaDeNegocioException("Compañía no encontrada.");
        compania.Activa = false;
        await _db.SaveChangesAsync();
    }

    public async Task EliminarCompania(int companiaId)
    {
        var tienePolizas = await _db.Polizas.AnyAsync(p => p.CompaniaId == companiaId);
        if (tienePolizas)
            throw new ReglaDeNegocioException("No se puede eliminar una compañía con pólizas asociadas. Inactívela en su lugar.");

        var compania = await _db.Companias.FindAsync(companiaId)
            ?? throw new ReglaDeNegocioException("Compañía no encontrada.");
        _db.Companias.Remove(compania);
        await _db.SaveChangesAsync();
    }
}
