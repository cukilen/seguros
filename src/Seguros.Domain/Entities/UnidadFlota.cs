namespace Seguros.Domain.Entities;

public class UnidadFlota
{
    public int Id { get; set; }
    public int PolizaId { get; set; }
    public Poliza Poliza { get; set; } = null!;

    public string Patente { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Uso { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;
    public DateOnly? FechaBaja { get; set; }
}
