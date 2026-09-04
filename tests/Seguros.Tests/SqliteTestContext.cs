using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Seguros.Data;

namespace Seguros.Tests;

/// <summary>
/// Base de datos SQLite en memoria para cada test: aislada, rápida, y sin tocar el
/// archivo real de la aplicación. Se mantiene una única conexión abierta porque
/// una base ":memory:" de SQLite desaparece cuando se cierra su conexión.
/// </summary>
public class SqliteTestContext : IDisposable
{
    private readonly SqliteConnection _connection;
    public SegurosDbContext Db { get; }

    public SqliteTestContext()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SegurosDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new SegurosDbContext(options);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
