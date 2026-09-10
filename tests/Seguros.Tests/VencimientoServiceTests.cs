using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Xunit;

namespace Seguros.Tests;

// specs/vencimientos/spec.md
public class VencimientoServiceTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();
    private readonly VencimientoService _service;
    private readonly PolizaService _polizas;
    private int _productorId, _aseguradoId, _companiaId;

    public VencimientoServiceTests()
    {
        _service = new VencimientoService(_ctx.Db);
        _polizas = new PolizaService(_ctx.Db);
        Preparar().GetAwaiter().GetResult();
    }

    private async Task Preparar()
    {
        var productor = await new ProductorService(_ctx.Db).AltaProductor("P", "p1", "clave");
        var asegurado = await new AseguradoService(_ctx.Db).AltaAsegurado(productor.Id, "Cliente", TipoDocumento.Dni, "20111222");
        var compania = await new CompaniaService(_ctx.Db).AltaCompania("La Segunda", new[] { _ctx.RamoAutosId });
        _productorId = productor.Id;
        _aseguradoId = asegurado.Id;
        _companiaId = compania.Id;
    }

    private Task<Domain.Entities.Poliza> CrearPoliza(string numero, DateOnly vigenciaHasta) =>
        _polizas.AltaPoliza(_productorId, _aseguradoId, _companiaId, _ctx.RamoAutosId, numero,
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), vigenciaHasta, 1000m);

    [Fact] // Póliza dentro de la ventana de alerta
    public async Task ListarProximasAVencer_incluyePolizaDentroDeLaVentana()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        await CrearPoliza("P-1", hoy.AddDays(10));
        await CrearPoliza("P-2", hoy.AddDays(90));

        var proximas = await _service.ListarProximasAVencer(diasVentana: 15);

        Assert.Single(proximas);
        Assert.Equal("P-1", proximas[0].Numero);
    }

    [Fact] // Una póliza ya vencida no debe listarse como "próxima a vencer"
    public async Task ListarProximasAVencer_noIncluyePolizaYaVencida()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        await CrearPoliza("P-1", hoy.AddDays(-30));
        await CrearPoliza("P-2", hoy.AddDays(10));

        var proximas = await _service.ListarProximasAVencer(diasVentana: 30);

        Assert.Single(proximas);
        Assert.Equal("P-2", proximas[0].Numero);
    }

    [Fact] // Filtrado de vencimientos por compañía
    public async Task ListarProximasAVencer_filtraPorCompania()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var otraCompania = await new CompaniaService(_ctx.Db).AltaCompania("Zurich", new[] { _ctx.RamoAutosId });
        await CrearPoliza("P-1", hoy.AddDays(5));
        await _polizas.AltaPoliza(_productorId, _aseguradoId, otraCompania.Id, _ctx.RamoAutosId, "P-2",
            hoy.AddYears(-1), hoy.AddDays(5), 1000m);

        var deLaCompania = await _service.ListarProximasAVencer(diasVentana: 30, companiaId: _companiaId);

        Assert.Single(deLaCompania);
        Assert.Equal(_companiaId, deLaCompania[0].CompaniaId);
    }

    [Fact] // Vencimiento resuelto por renovación
    public async Task MarcarComoGestionado_dejaDeListarseComoPendiente()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var poliza = await CrearPoliza("P-1", hoy.AddDays(5));

        await _service.MarcarComoGestionado(poliza.Id);

        var proximas = await _service.ListarProximasAVencer(diasVentana: 30);
        Assert.Empty(proximas);
    }

    [Fact] // Listado de pólizas vencidas
    public async Task ListarVencidas_incluyeSoloPolizasConFechaPasada()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        await CrearPoliza("P-1", hoy.AddDays(-5));
        await CrearPoliza("P-2", hoy.AddDays(30));

        var vencidas = await _service.ListarVencidas();

        Assert.Single(vencidas);
        Assert.Equal("P-1", vencidas[0].Numero);
    }

    public void Dispose() => _ctx.Dispose();
}
