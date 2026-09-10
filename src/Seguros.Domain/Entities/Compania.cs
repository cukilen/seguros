namespace Seguros.Domain.Entities;

public class Compania
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activa { get; set; } = true;

    public List<CompaniaRamo> RamosOperados { get; set; } = new();
    public List<Poliza> Polizas { get; set; } = new();
}

/// <summary>Ramo que una compañía opera, y el % de comisión pactado para esa combinación.</summary>
public class CompaniaRamo
{
    public int Id { get; set; }
    public int CompaniaId { get; set; }
    public Compania Compania { get; set; } = null!;

    public int RamoId { get; set; }
    public Ramo Ramo { get; set; } = null!;

    /// <summary>Null = todavía no se pactó comisión para esta combinación compañía/ramo.</summary>
    public decimal? PorcentajeComision { get; set; }
}
