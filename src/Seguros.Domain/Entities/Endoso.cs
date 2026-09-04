namespace Seguros.Domain.Entities;

public class Endoso
{
    public int Id { get; set; }
    public int PolizaId { get; set; }
    public Poliza Poliza { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;

    public bool Anulado { get; set; }
    public string? MotivoAnulacion { get; set; }
}
