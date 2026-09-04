using Microsoft.Data.Sqlite;

namespace Seguros.Data.Services;

/// <summary>
/// Backup manual del archivo SQLite (tasks.md - 12.1). Usa la API de backup de
/// SQLite en lugar de copiar el archivo directamente, para obtener una copia
/// consistente aunque haya una conexión abierta.
/// </summary>
public class BackupService
{
    private readonly string _cadenaConexionOrigen;

    public BackupService(string? cadenaConexionOrigen = null) =>
        _cadenaConexionOrigen = cadenaConexionOrigen ?? RutaBaseDeDatos.CadenaConexion();

    public void HacerBackup(string rutaDestino)
    {
        using var origen = new SqliteConnection(_cadenaConexionOrigen);
        origen.Open();

        using var destino = new SqliteConnection($"Data Source={rutaDestino}");
        destino.Open();

        origen.BackupDatabase(destino);
    }
}
