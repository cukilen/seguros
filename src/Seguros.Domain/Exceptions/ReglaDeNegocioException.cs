namespace Seguros.Domain.Exceptions;

/// <summary>
/// Se lanza cuando una operación viola una regla de negocio descrita en las specs
/// (ej. documento duplicado, número de póliza repetido, baja con pólizas vigentes).
/// </summary>
public class ReglaDeNegocioException : Exception
{
    public ReglaDeNegocioException(string message) : base(message) { }
}
