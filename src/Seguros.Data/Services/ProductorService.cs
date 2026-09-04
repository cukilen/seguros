using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Exceptions;

namespace Seguros.Data.Services;

/// <summary>Implementa specs/productores: alta, login, aislamiento de cartera, edición y baja.</summary>
public class ProductorService
{
    private readonly SegurosDbContext _db;

    public ProductorService(SegurosDbContext db) => _db = db;

    public async Task<Productor> AltaProductor(string nombre, string nombreUsuario, string password)
    {
        nombreUsuario = NormalizarUsuario(nombreUsuario);

        if (await _db.Productores.AnyAsync(p => p.NombreUsuario == nombreUsuario))
            throw new ReglaDeNegocioException($"Ya existe un productor con el usuario '{nombreUsuario}'.");

        var productor = new Productor
        {
            Nombre = nombre,
            NombreUsuario = nombreUsuario,
            PasswordHash = PasswordHasher.Hash(password),
            Activo = true
        };

        _db.Productores.Add(productor);
        await _db.SaveChangesAsync();
        return productor;
    }

    /// <summary>Devuelve el productor si las credenciales son válidas y está activo; null en caso contrario.</summary>
    public async Task<Productor?> Login(string nombreUsuario, string password)
    {
        nombreUsuario = NormalizarUsuario(nombreUsuario);
        var productor = await _db.Productores.FirstOrDefaultAsync(p => p.NombreUsuario == nombreUsuario);
        if (productor is null || !productor.Activo) return null;
        return PasswordHasher.Verificar(password, productor.PasswordHash) ? productor : null;
    }

    /// <summary>El usuario de acceso no distingue mayúsculas/minúsculas ni espacios al principio/final.</summary>
    private static string NormalizarUsuario(string nombreUsuario) => nombreUsuario.Trim().ToLowerInvariant();

    public async Task EditarProductor(int id, string nombre)
    {
        var productor = await _db.Productores.FindAsync(id)
            ?? throw new ReglaDeNegocioException("Productor no encontrado.");
        productor.Nombre = nombre;
        await _db.SaveChangesAsync();
    }

    public async Task DesactivarProductor(int id)
    {
        var productor = await _db.Productores.FindAsync(id)
            ?? throw new ReglaDeNegocioException("Productor no encontrado.");
        productor.Activo = false;
        await _db.SaveChangesAsync();
    }
}
