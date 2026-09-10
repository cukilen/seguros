using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Seguros.Data;
using Seguros.Data.Services;
using Xunit;

namespace Seguros.Tests;

// tasks.md 12.1: backup manual del archivo de base de datos
public class BackupServiceTests
{
    [Fact]
    public async Task HacerBackup_generaUnArchivoValidoYRestaurable()
    {
        var rutaOrigen = Path.Combine(Path.GetTempPath(), $"seguros-origen-{Guid.NewGuid():N}.db");
        var rutaDestino = Path.Combine(Path.GetTempPath(), $"seguros-backup-{Guid.NewGuid():N}.db");

        try
        {
            var cadenaOrigen = $"Data Source={rutaOrigen}";
            var options = new DbContextOptionsBuilder<SegurosDbContext>().UseSqlite(cadenaOrigen).Options;
            await using (var db = new SegurosDbContext(options))
            {
                await db.Database.EnsureCreatedAsync();
                var ramoAutosId = db.Ramos.Single(r => r.Nombre == "Autos").Id;
                await new CompaniaService(db).AltaCompania("La Segunda", new[] { ramoAutosId });
            }

            new BackupService(cadenaOrigen).HacerBackup(rutaDestino);

            Assert.True(File.Exists(rutaDestino));

            using var conexionRestaurada = new SqliteConnection($"Data Source={rutaDestino}");
            conexionRestaurada.Open();
            using var comando = conexionRestaurada.CreateCommand();
            comando.CommandText = "SELECT COUNT(*) FROM Companias";
            var cantidad = (long)comando.ExecuteScalar()!;

            Assert.Equal(1, cantidad);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(rutaOrigen)) File.Delete(rutaOrigen);
            if (File.Exists(rutaDestino)) File.Delete(rutaDestino);
        }
    }
}
