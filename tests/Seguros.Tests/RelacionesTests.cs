using Microsoft.EntityFrameworkCore;
using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Xunit;

namespace Seguros.Tests;

// tasks.md 2.2: relaciones clave del modelo de datos
public class RelacionesTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();

    [Fact]
    public async Task RelacionesClave_sePuedenCrearYConsultar()
    {
        var productor = await new ProductorService(_ctx.Db).AltaProductor("P", "p1", "clave");
        var asegurado = await new AseguradoService(_ctx.Db).AltaAsegurado(productor.Id, "Cliente", "20111222");
        var compania = await new CompaniaService(_ctx.Db).AltaCompania("La Segunda", new[] { Ramo.Autos });
        var poliza = await new PolizaService(_ctx.Db).AltaPoliza(productor.Id, asegurado.Id, compania.Id, Ramo.Autos,
            "P-1", DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 1000m);
        await new FlotaService(_ctx.Db).MarcarComoFlota(poliza.Id);
        var unidad = await new FlotaService(_ctx.Db).AltaUnidad(poliza.Id, "AB123CD", "Ford", "Transit", "Carga");
        var endoso = await new EndosoService(_ctx.Db).AltaEndoso(poliza.Id, DateOnly.FromDateTime(DateTime.Today), "Tipo", "Detalle");

        var polizaCargada = await _ctx.Db.Polizas
            .Include(p => p.Productor)
            .Include(p => p.Asegurado)
            .Include(p => p.Compania)
            .Include(p => p.Unidades)
            .Include(p => p.Endosos)
            .SingleAsync(p => p.Id == poliza.Id);

        Assert.Equal(productor.Id, polizaCargada.Productor.Id);
        Assert.Equal(asegurado.Id, polizaCargada.Asegurado.Id);
        Assert.Equal(compania.Id, polizaCargada.Compania.Id);
        Assert.Contains(polizaCargada.Unidades, u => u.Id == unidad.Id);
        Assert.Contains(polizaCargada.Endosos, e => e.Id == endoso.Id);
    }

    public void Dispose() => _ctx.Dispose();
}
