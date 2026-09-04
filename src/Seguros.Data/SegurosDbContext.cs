using Microsoft.EntityFrameworkCore;
using Seguros.Domain.Entities;

namespace Seguros.Data;

public class SegurosDbContext : DbContext
{
    public DbSet<Productor> Productores => Set<Productor>();
    public DbSet<Compania> Companias => Set<Compania>();
    public DbSet<CompaniaRamo> CompaniaRamos => Set<CompaniaRamo>();
    public DbSet<Asegurado> Asegurados => Set<Asegurado>();
    public DbSet<Poliza> Polizas => Set<Poliza>();
    public DbSet<UnidadFlota> UnidadesFlota => Set<UnidadFlota>();
    public DbSet<Endoso> Endosos => Set<Endoso>();
    public DbSet<Liquidacion> Liquidaciones => Set<Liquidacion>();
    public DbSet<LiquidacionDetalle> LiquidacionDetalles => Set<LiquidacionDetalle>();

    public SegurosDbContext(DbContextOptions<SegurosDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Productor>()
            .HasIndex(p => p.NombreUsuario)
            .IsUnique();

        modelBuilder.Entity<CompaniaRamo>()
            .HasIndex(cr => new { cr.CompaniaId, cr.Ramo })
            .IsUnique();

        modelBuilder.Entity<Asegurado>()
            .HasIndex(a => new { a.ProductorId, a.Documento })
            .IsUnique();

        modelBuilder.Entity<Poliza>()
            .HasIndex(p => new { p.CompaniaId, p.Numero })
            .IsUnique();

        modelBuilder.Entity<Poliza>()
            .HasOne(p => p.PolizaOrigen)
            .WithMany()
            .HasForeignKey(p => p.PolizaOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Poliza>()
            .Property(p => p.Prima)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<CompaniaRamo>()
            .Property(cr => cr.PorcentajeComision)
            .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<Liquidacion>()
            .Property(l => l.MontoTotal)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<LiquidacionDetalle>()
            .Property(d => d.MontoComision)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<LiquidacionDetalle>()
            .HasOne(d => d.Poliza)
            .WithMany()
            .HasForeignKey(d => d.PolizaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
