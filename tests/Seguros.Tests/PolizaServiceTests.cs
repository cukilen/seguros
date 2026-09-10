using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;
using Xunit;

namespace Seguros.Tests;

// specs/polizas/spec.md
public class PolizaServiceTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();
    private readonly PolizaService _service;
    private int _productorId;
    private int _aseguradoId;
    private int _companiaId;

    public PolizaServiceTests()
    {
        _service = new PolizaService(_ctx.Db);
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

    private Task<Domain.Entities.Poliza> CrearPoliza(string numero = "P-1", decimal prima = 1000m) =>
        _service.AltaPoliza(_productorId, _aseguradoId, _companiaId, _ctx.RamoAutosId, numero,
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), prima);

    [Fact] // Alta exitosa
    public async Task AltaPoliza_conDatosValidos_quedaVigente()
    {
        var poliza = await CrearPoliza();

        Assert.Equal(EstadoPoliza.Vigente, poliza.Estado);
    }

    [Fact] // Alta con producto, premio y cobertura
    public async Task AltaPoliza_conProductoPremioYCobertura_losGuarda()
    {
        var poliza = await _service.AltaPoliza(_productorId, _aseguradoId, _companiaId, _ctx.RamoAutosId, "P-9",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 1000m,
            producto: "Clásico Trimestral", premio: 1300m, cobertura: "A -Responsabilidad Civil",
            descripcionRiesgo: "Ford Fiesta 2019");

        Assert.Equal("Clásico Trimestral", poliza.Producto);
        Assert.Equal(1300m, poliza.Premio);
        Assert.Equal("A -Responsabilidad Civil", poliza.Cobertura);
        Assert.Equal("Ford Fiesta 2019", poliza.DescripcionRiesgo);
    }

    [Fact] // Número de póliza duplicado en la misma compañía
    public async Task AltaPoliza_conNumeroDuplicadoEnMismaCompania_esRechazada()
    {
        await CrearPoliza("P-1");

        await Assert.ThrowsAsync<ReglaDeNegocioException>(() => CrearPoliza("P-1"));
    }

    [Fact] // Edición de la prima de una póliza
    public async Task EditarPoliza_actualizaPrimaYVigencia()
    {
        var poliza = await CrearPoliza();

        await _service.EditarPoliza(poliza.Id, 2500m, DateOnly.FromDateTime(DateTime.Today.AddYears(2)));

        var actualizada = (await _service.Consultar(aseguradoId: _aseguradoId)).Single();
        Assert.Equal(2500m, actualizada.Prima);
    }

    [Fact] // Renovación exitosa
    public async Task RenovarPoliza_generaNuevaVigenciaVinculadaALaOriginal()
    {
        var original = await CrearPoliza("P-1");

        var renovada = await _service.RenovarPoliza(original.Id, "P-1-R1",
            original.VigenciaHasta, original.VigenciaHasta.AddYears(1), 1200m);

        Assert.Equal(original.Id, renovada.PolizaOrigenId);
        Assert.Equal(EstadoPoliza.Vigente, renovada.Estado);

        var originalActualizada = (await _service.Consultar(aseguradoId: _aseguradoId))
            .Single(p => p.Id == original.Id);
        Assert.Equal(EstadoPoliza.Renovada, originalActualizada.Estado);
    }

    [Fact] // Anulación de póliza
    public async Task AnularPoliza_dejaDeListarseEntreVigentes()
    {
        var poliza = await CrearPoliza();

        await _service.AnularPoliza(poliza.Id, "Pedido del cliente");

        var vigentes = (await _service.Consultar(aseguradoId: _aseguradoId))
            .Where(p => p.Estado == EstadoPoliza.Vigente);
        Assert.Empty(vigentes);
    }

    [Fact] // Póliza vencida sin gestión
    public async Task MarcarComoNoVigente_dejaDeContarseComoVigenteSinQuedarAnulada()
    {
        var poliza = await CrearPoliza();

        await _service.MarcarComoNoVigente(poliza.Id);

        var actualizada = (await _service.Consultar(aseguradoId: _aseguradoId)).Single();
        Assert.Equal(EstadoPoliza.NoVigente, actualizada.Estado);
        Assert.NotEqual(EstadoPoliza.Anulada, actualizada.Estado);
    }

    [Fact] // Filtrado por ramo
    public async Task Consultar_filtraPorRamo()
    {
        await CrearPoliza("P-1");
        var companiaVida = await new CompaniaService(_ctx.Db).AltaCompania("Zurich", new[] { _ctx.RamoVidaId });
        await _service.AltaPoliza(_productorId, _aseguradoId, companiaVida.Id, _ctx.RamoVidaId, "V-1",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 800m);

        var soloAutos = await _service.Consultar(ramoId: _ctx.RamoAutosId);

        Assert.All(soloAutos, p => Assert.Equal(_ctx.RamoAutosId, p.RamoId));
    }

    public void Dispose() => _ctx.Dispose();
}
