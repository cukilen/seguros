using System.Security.Cryptography;

namespace Seguros.Data.Services;

/// <summary>Hash de contraseñas con PBKDF2 (sin dependencias externas).</summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iteraciones = 100_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string password, string hashAlmacenado)
    {
        var partes = hashAlmacenado.Split('.');
        if (partes.Length != 2) return false;

        var salt = Convert.FromBase64String(partes[0]);
        var hashEsperado = Convert.FromBase64String(partes[1]);
        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}
