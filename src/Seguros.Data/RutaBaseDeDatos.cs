namespace Seguros.Data;

/// <summary>
/// Resuelve dónde vive el archivo SQLite de la aplicación. Un solo archivo local,
/// sin servidor de base de datos que instalar (ver design.md - Decisión 3).
/// </summary>
public static class RutaBaseDeDatos
{
    public static string ObtenerRutaArchivo()
    {
        var carpetaDatos = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Seguros");

        Directory.CreateDirectory(carpetaDatos);
        return Path.Combine(carpetaDatos, "seguros.db");
    }

    public static string CadenaConexion() => $"Data Source={ObtenerRutaArchivo()}";
}
