using Seguros.Domain.Enums;

namespace Seguros.Domain.Entities;

public class Poliza
{
    public int Id { get; set; }

    public int ProductorId { get; set; }
    public Productor Productor { get; set; } = null!;

    public int AseguradoId { get; set; }
    public Asegurado Asegurado { get; set; } = null!;

    public int CompaniaId { get; set; }
    public Compania Compania { get; set; } = null!;

    public int RamoId { get; set; }
    public Ramo Ramo { get; set; } = null!;

    public string Numero { get; set; } = string.Empty;
    public DateOnly VigenciaDesde { get; set; }
    public DateOnly VigenciaHasta { get; set; }
    public decimal Prima { get; set; }

    /// <summary>Nombre comercial del plan (ej. "Clásico Trimestral"). No todas las fuentes lo traen.</summary>
    public string? Producto { get; set; }

    /// <summary>Monto total de la póliza incluyendo impuestos/recargos, distinto de la prima neta.</summary>
    public decimal? Premio { get; set; }

    /// <summary>Nivel de cobertura (relevante en autos: A, B, B1, C, CL, CP...).</summary>
    public string? Cobertura { get; set; }

    /// <summary>Descripción del riesgo asegurado (ej. un vehículo) para pólizas que no son de flota.</summary>
    public string? DescripcionRiesgo { get; set; }

    public EstadoPoliza Estado { get; set; } = EstadoPoliza.Vigente;
    public string? MotivoAnulacion { get; set; }

    /// <summary>True si esta póliza es de flota (ramo Autos) y admite múltiples unidades.</summary>
    public bool EsFlota { get; set; }

    /// <summary>Póliza de la que proviene esta, cuando fue creada por una renovación.</summary>
    public int? PolizaOrigenId { get; set; }
    public Poliza? PolizaOrigen { get; set; }

    /// <summary>El vencimiento de esta póliza ya fue gestionado (renovada o dada de baja a propósito).</summary>
    public bool VencimientoGestionado { get; set; }

    public List<UnidadFlota> Unidades { get; set; } = new();
    public List<Endoso> Endosos { get; set; } = new();
}
