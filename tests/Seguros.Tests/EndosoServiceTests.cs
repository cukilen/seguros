using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;
using Xunit;

namespace Seguros.Tests;

// specs/endosos/spec.md
public class EndosoServiceTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();
    private readonly EndosoService _service;
    private readonly PolizaService _polizas;
    private int _polizaId;

    public EndosoServiceTests()
    {
        _service = new EndosoService(_ctx.Db);
        _polizas = new PolizaService(_ctx.Db);
        _polizaId = Preparar().GetAwaiter().GetResult();
    }

    private async Task<int> Preparar()
    {
        var productor = await new ProductorService(_ctx.Db).AltaProductor("P", "p1", "clave");
        var asegurado = await new AseguradoService(_ctx.Db).AltaAsegurado(productor.Id, "Cliente", TipoDocumento.Dni, "20111222");
        var compania = await new CompaniaService(_ctx.Db).AltaCompania("La Segunda", new[] { _ctx.RamoAutosId });
        var poliza = await _polizas.AltaPoliza(productor.Id, asegurado.Id, compania.Id, _ctx.RamoAutosId, "P-1",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 1000m);
        return poliza.Id;
    }

    [Fact] // Alta de endoso exitosa
    public async Task AltaEndoso_sobrePolizaVigente_seRegistra()
    {
        var endoso = await _service.AltaEndoso(_polizaId, DateOnly.FromDateTime(DateTime.Today), "Cambio de domicilio", "Nuevo domicilio del asegurado");

        Assert.NotEqual(0, endoso.Id);
    }

    [Fact] // Endoso sobre póliza anulada
    public async Task AltaEndoso_sobrePolizaAnulada_esRechazada()
    {
        await _polizas.AnularPoliza(_polizaId, "Pedido del cliente");

        await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => _service.AltaEndoso(_polizaId, DateOnly.FromDateTime(DateTime.Today), "Cambio", "Detalle"));
    }

    [Fact] // Consulta del historial ordenado cronológicamente
    public async Task HistorialEndosos_ordenaCronologicamente()
    {
        await _service.AltaEndoso(_polizaId, new DateOnly(2026, 3, 1), "Tipo A", "Detalle A");
        await _service.AltaEndoso(_polizaId, new DateOnly(2026, 1, 1), "Tipo B", "Detalle B");

        var historial = await _service.HistorialEndosos(_polizaId);

        Assert.Equal("Tipo B", historial[0].Tipo);
        Assert.Equal("Tipo A", historial[1].Tipo);
    }

    [Fact] // Anulación de endoso
    public async Task AnularEndoso_quedaVisibleComoAnulado()
    {
        var endoso = await _service.AltaEndoso(_polizaId, DateOnly.FromDateTime(DateTime.Today), "Tipo A", "Detalle");

        await _service.AnularEndoso(endoso.Id, "Cargado por error");

        var historial = await _service.HistorialEndosos(_polizaId);
        Assert.True(historial.Single().Anulado);
    }

    public void Dispose() => _ctx.Dispose();
}
