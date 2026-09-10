using Seguros.Domain.Entities;

namespace Seguros.Data;

/// <summary>
/// Catálogo inicial de ramos (tasks.md - 4.3), con los nombres observados en los
/// 3 archivos reales analizados (cartera vigente, pólizas en cartera y libro de
/// movimientos) para no arrancar de un catálogo vacío.
/// </summary>
public static class SembrarRamos
{
    public static readonly Ramo[] Catalogo =
    {
        new() { Id = 1, Nombre = "Autos" },
        new() { Id = 2, Nombre = "Motos" },
        new() { Id = 3, Nombre = "Vida" },
        new() { Id = 4, Nombre = "Vida Colectivo" },
        new() { Id = 5, Nombre = "Vida Individual" },
        new() { Id = 6, Nombre = "Vida Obligatorio" },
        new() { Id = 7, Nombre = "Incendio" },
        new() { Id = 8, Nombre = "Combinado Familiar" },
        new() { Id = 9, Nombre = "Responsabilidad Civil" },
        new() { Id = 10, Nombre = "Accidentes Personales" },
        new() { Id = 11, Nombre = "Integral de Comercio" },
        new() { Id = 12, Nombre = "Caución" },
        new() { Id = 13, Nombre = "Transporte / Cascos" },
    };
}
