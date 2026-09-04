namespace Seguros.Domain.Entities;

public class Liquidacion
{
    public int Id { get; set; }
    public int ProductorId { get; set; }
    public Productor Productor { get; set; } = null!;

    public DateOnly PeriodoDesde { get; set; }
    public DateOnly PeriodoHasta { get; set; }
    public decimal MontoTotal { get; set; }
    public DateTime FechaGeneracion { get; set; }

    public List<LiquidacionDetalle> Detalles { get; set; } = new();
}

/// <summary>Comisión de una póliza puntual incluida dentro de una liquidación.</summary>
public class LiquidacionDetalle
{
    public int Id { get; set; }
    public int LiquidacionId { get; set; }
    public Liquidacion Liquidacion { get; set; } = null!;

    public int PolizaId { get; set; }
    public Poliza Poliza { get; set; } = null!;

    public decimal MontoComision { get; set; }
}
