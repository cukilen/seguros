using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Seguros.Data;

/// <summary>Usada por las herramientas de diseño de EF Core (dotnet-ef) para generar migraciones.</summary>
public class SegurosDbContextFactory : IDesignTimeDbContextFactory<SegurosDbContext>
{
    public SegurosDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SegurosDbContext>()
            .UseSqlite(RutaBaseDeDatos.CadenaConexion())
            .Options;

        return new SegurosDbContext(options);
    }
}
