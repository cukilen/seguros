using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Xunit;

namespace Seguros.Tests;

// specs/facturacion-comisiones/spec.md
public class ComisionServiceTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();
    private readonly ComisionService _service;
    private readonly PolizaService _polizas;
    private int _productorId, _aseguradoId, _companiaId;

    public ComisionServiceTests()
    {
        _service = new ComisionService(_ctx.Db);
        _polizas = new PolizaService(_ctx.Db);
        Preparar().GetAwaiter().GetResult();
    }

    private async Task Preparar()
    {
        var productor = await new ProductorService(_ctx.Db).AltaProductor("P", "p1", "clave");
        var asegurado = await new AseguradoService(_ctx.Db).AltaAsegurado(productor.Id, "Cliente", "20111222");
        var compania = await new CompaniaService(_ctx.Db).AltaCompania("La Segunda", new[] { Ramo.Autos });
        _productorId = productor.Id;
        _aseguradoId = asegurado.Id;
        _companiaId = compania.Id;
    }

    [Fact] // Alta de comisión pactada + cálculo automático al emitir una póliza
    public async Task CalcularComision_usaElPorcentajePactadoParaCompaniaYRamo()
    {
        await _service.ConfigurarComision(_companiaId, Ramo.Autos, 10m);
        var poliza = await _polizas.AltaPoliza(_productorId, _aseguradoId, _companiaId, Ramo.Autos, "P-1",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 1000m);

        var comision = await _service.CalcularComision(poliza.Id);

        Assert.Equal(100m, comision);
    }

    [Fact] // Póliza sin porcentaje de comisión configurado
    public async Task CalcularComision_sinPorcentajeConfigurado_devuelveNull()
    {
        var poliza = await _polizas.AltaPoliza(_productorId, _aseguradoId, _companiaId, Ramo.Autos, "P-1",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 1000m);

        var comision = await _service.CalcularComision(poliza.Id);

        Assert.Null(comision);
    }

    [Fact] // Generación de liquidación de un período
    public async Task GenerarLiquidacion_totalizaLasPolizasDelPeriodo()
    {
        await _service.ConfigurarComision(_companiaId, Ramo.Autos, 10m);
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        await _polizas.AltaPoliza(_productorId, _aseguradoId, _companiaId, Ramo.Autos, "P-1", hoy, hoy.AddYears(1), 1000m);
        await _polizas.AltaPoliza(_productorId, _aseguradoId, _companiaId, Ramo.Autos, "P-2", hoy, hoy.AddYears(1), 2000m);

        var liquidacion = await _service.GenerarLiquidacion(_productorId, hoy.AddDays(-1), hoy.AddDays(1));

        Assert.Equal(300m, liquidacion.MontoTotal);
        Assert.Equal(2, liquidacion.Detalles.Count);
    }

    [Fact] // Consulta de liquidaciones previas
    public async Task HistorialLiquidaciones_muestraLasGeneradasAnteriormente()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        await _service.GenerarLiquidacion(_productorId, hoy.AddMonths(-1), hoy);

        var historial = await _service.HistorialLiquidaciones(_productorId);

        Assert.Single(historial);
    }

    public void Dispose() => _ctx.Dispose();
}
