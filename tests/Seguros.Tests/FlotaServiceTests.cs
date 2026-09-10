using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;
using Xunit;

namespace Seguros.Tests;

// specs/flotas/spec.md
public class FlotaServiceTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();
    private readonly FlotaService _service;
    private int _polizaId;

    public FlotaServiceTests()
    {
        _service = new FlotaService(_ctx.Db);
        _polizaId = Preparar().GetAwaiter().GetResult();
    }

    private async Task<int> Preparar()
    {
        var productor = await new ProductorService(_ctx.Db).AltaProductor("P", "p1", "clave");
        var asegurado = await new AseguradoService(_ctx.Db).AltaAsegurado(productor.Id, "Cliente", TipoDocumento.Dni, "20111222");
        var compania = await new CompaniaService(_ctx.Db).AltaCompania("La Segunda", new[] { _ctx.RamoAutosId });
        var poliza = await new PolizaService(_ctx.Db).AltaPoliza(productor.Id, asegurado.Id, compania.Id, _ctx.RamoAutosId,
            "P-1", DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 50000m);
        return poliza.Id;
    }

    [Fact] // Alta de póliza de flota
    public async Task MarcarComoFlota_habilitaAltaDeUnidades()
    {
        await _service.MarcarComoFlota(_polizaId);

        var unidad = await _service.AltaUnidad(_polizaId, "AB123CD", "Ford", "Transit", "Carga");

        Assert.NotEqual(0, unidad.Id);
    }

    [Fact] // Patente duplicada dentro de la misma flota
    public async Task AltaUnidad_conPatenteDuplicadaEnLaFlota_esRechazada()
    {
        await _service.MarcarComoFlota(_polizaId);
        await _service.AltaUnidad(_polizaId, "AB123CD", "Ford", "Transit", "Carga");

        await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => _service.AltaUnidad(_polizaId, "AB123CD", "Ford", "Transit", "Carga"));
    }

    [Fact] // Baja de una unidad
    public async Task BajaUnidad_noAfectaElRestoDeLaFlota()
    {
        await _service.MarcarComoFlota(_polizaId);
        var unidad1 = await _service.AltaUnidad(_polizaId, "AB123CD", "Ford", "Transit", "Carga");
        var unidad2 = await _service.AltaUnidad(_polizaId, "EF456GH", "Fiat", "Cronos", "Particular");

        await _service.BajaUnidad(unidad1.Id, DateOnly.FromDateTime(DateTime.Today));

        var unidades = await _service.ListarUnidades(_polizaId);
        Assert.False(unidades.Single(u => u.Id == unidad1.Id).Activa);
        Assert.True(unidades.Single(u => u.Id == unidad2.Id).Activa);
    }

    [Fact] // Consulta del listado de unidades
    public async Task ListarUnidades_distingueActivasEInactivas()
    {
        await _service.MarcarComoFlota(_polizaId);
        var unidad = await _service.AltaUnidad(_polizaId, "AB123CD", "Ford", "Transit", "Carga");
        await _service.BajaUnidad(unidad.Id, DateOnly.FromDateTime(DateTime.Today));
        await _service.AltaUnidad(_polizaId, "EF456GH", "Fiat", "Cronos", "Particular");

        var unidades = await _service.ListarUnidades(_polizaId);

        Assert.Equal(2, unidades.Count);
        Assert.Single(unidades, u => u.Activa);
        Assert.Single(unidades, u => !u.Activa);
    }

    public void Dispose() => _ctx.Dispose();
}
