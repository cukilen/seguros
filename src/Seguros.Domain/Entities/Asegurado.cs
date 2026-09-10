using Seguros.Domain.Enums;

namespace Seguros.Domain.Entities;

public class Asegurado
{
    public int Id { get; set; }
    public int ProductorId { get; set; }
    public Productor Productor { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Documento opcional: no todas las fuentes de datos lo proveen (specs/asegurados).</summary>
    public TipoDocumento? TipoDocumento { get; set; }
    public string? NroDocumento { get; set; }

    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Domicilio { get; set; }
    public bool Activo { get; set; } = true;

    public List<Poliza> Polizas { get; set; } = new();
}
