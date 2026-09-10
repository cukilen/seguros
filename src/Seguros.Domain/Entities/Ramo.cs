namespace Seguros.Domain.Entities;

/// <summary>
/// Catálogo abierto de ramos/secciones de seguro (specs/ramos). Reemplaza el enum
/// cerrado original: las compañías reales usan muchos más nombres de los previstos.
/// </summary>
public class Ramo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
