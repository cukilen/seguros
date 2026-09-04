namespace Seguros.Domain.Entities;

public class Productor
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public List<Asegurado> Asegurados { get; set; } = new();
    public List<Poliza> Polizas { get; set; } = new();
}
