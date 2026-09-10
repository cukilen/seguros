using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;

namespace Seguros.Data.Services;

/// <summary>Implementa specs/asegurados.</summary>
public class AseguradoService
{
    private readonly SegurosDbContext _db;

    public AseguradoService(SegurosDbContext db) => _db = db;

    public async Task<Asegurado> AltaAsegurado(int productorId, string nombre, TipoDocumento? tipoDocumento = null,
        string? nroDocumento = null, string? telefono = null, string? email = null, string? domicilio = null)
    {
        if (tipoDocumento is null != string.IsNullOrWhiteSpace(nroDocumento))
            throw new ReglaDeNegocioException("Indicá tipo y número de documento juntos, o dejá ambos vacíos.");

        if (tipoDocumento is not null)
        {
            var yaExiste = await _db.Asegurados.AnyAsync(a =>
                a.ProductorId == productorId && a.TipoDocumento == tipoDocumento && a.NroDocumento == nroDocumento);
            if (yaExiste)
                throw new ReglaDeNegocioException($"Ya existe un asegurado con el documento '{nroDocumento}' para este productor.");
        }

        var asegurado = new Asegurado
        {
            ProductorId = productorId,
            Nombre = nombre,
            TipoDocumento = tipoDocumento,
            NroDocumento = string.IsNullOrWhiteSpace(nroDocumento) ? null : nroDocumento,
            Telefono = telefono,
            Email = email,
            Domicilio = domicilio,
            Activo = true
        };

        _db.Asegurados.Add(asegurado);
        await _db.SaveChangesAsync();
        return asegurado;
    }

    public async Task EditarAsegurado(int aseguradoId, string? telefono, string? email, string? domicilio)
    {
        var asegurado = await _db.Asegurados.FindAsync(aseguradoId)
            ?? throw new ReglaDeNegocioException("Asegurado no encontrado.");
        asegurado.Telefono = telefono;
        asegurado.Email = email;
        asegurado.Domicilio = domicilio;
        await _db.SaveChangesAsync();
    }

    /// <summary>Vista consolidada: todas las pólizas del asegurado, sin importar compañía o ramo.</summary>
    public async Task<List<Poliza>> ObtenerPolizasDeAsegurado(int aseguradoId) =>
        await _db.Polizas
            .Include(p => p.Compania)
            .Include(p => p.Ramo)
            .Where(p => p.AseguradoId == aseguradoId)
            .OrderByDescending(p => p.VigenciaDesde)
            .ToListAsync();

    public async Task<List<Asegurado>> Buscar(int productorId, string texto)
    {
        texto = texto.Trim();
        return await _db.Asegurados
            .Where(a => a.ProductorId == productorId &&
                        (a.Nombre.Contains(texto) || (a.NroDocumento != null && a.NroDocumento.Contains(texto))))
            .ToListAsync();
    }

    public async Task InactivarAsegurado(int aseguradoId)
    {
        var asegurado = await _db.Asegurados.FindAsync(aseguradoId)
            ?? throw new ReglaDeNegocioException("Asegurado no encontrado.");
        asegurado.Activo = false;
        await _db.SaveChangesAsync();
    }

    public async Task EliminarAsegurado(int aseguradoId)
    {
        var tienePolizasVigentes = await _db.Polizas
            .AnyAsync(p => p.AseguradoId == aseguradoId && p.Estado == EstadoPoliza.Vigente);
        if (tienePolizasVigentes)
            throw new ReglaDeNegocioException("No se puede eliminar un asegurado con pólizas vigentes. Inactívelo en su lugar.");

        var asegurado = await _db.Asegurados.FindAsync(aseguradoId)
            ?? throw new ReglaDeNegocioException("Asegurado no encontrado.");
        _db.Asegurados.Remove(asegurado);
        await _db.SaveChangesAsync();
    }
}
