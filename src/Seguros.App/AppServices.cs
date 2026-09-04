using Microsoft.EntityFrameworkCore;
using Seguros.Data;
using Seguros.Data.Importacion;
using Seguros.Data.Services;
using Seguros.Domain.Entities;

namespace Seguros.App;

/// <summary>
/// Punto único de acceso a la base de datos y a los servicios de negocio para toda la UI.
/// App de escritorio simple: una sola instancia de DbContext para toda la sesión (ver design.md).
/// </summary>
public static class AppServices
{
    public static SegurosDbContext Db { get; private set; } = null!;
    public static ProductorService Productores { get; private set; } = null!;
    public static CompaniaService Companias { get; private set; } = null!;
    public static AseguradoService Asegurados { get; private set; } = null!;
    public static PolizaService Polizas { get; private set; } = null!;
    public static FlotaService Flotas { get; private set; } = null!;
    public static EndosoService Endosos { get; private set; } = null!;
    public static VencimientoService Vencimientos { get; private set; } = null!;
    public static ComisionService Comisiones { get; private set; } = null!;
    public static ImportacionService Importacion { get; private set; } = null!;
    public static BackupService Backup { get; private set; } = null!;

    /// <summary>Productor con la sesión iniciada (specs/productores).</summary>
    public static Productor ProductorActual { get; set; } = null!;

    public static void Inicializar()
    {
        var options = new DbContextOptionsBuilder<SegurosDbContext>()
            .UseSqlite(RutaBaseDeDatos.CadenaConexion())
            .Options;

        Db = new SegurosDbContext(options);
        Db.Database.Migrate();

        Productores = new ProductorService(Db);
        Companias = new CompaniaService(Db);
        Asegurados = new AseguradoService(Db);
        Polizas = new PolizaService(Db);
        Flotas = new FlotaService(Db);
        Endosos = new EndosoService(Db);
        Vencimientos = new VencimientoService(Db);
        Comisiones = new ComisionService(Db);
        Importacion = new ImportacionService(Db);
        Backup = new BackupService();
    }
}
