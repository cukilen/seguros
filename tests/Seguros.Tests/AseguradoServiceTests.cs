using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;
using Xunit;

namespace Seguros.Tests;

// specs/asegurados/spec.md
public class AseguradoServiceTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();
    private readonly AseguradoService _service;
    private readonly ProductorService _productores;
    private readonly PolizaService _polizas;
    private readonly CompaniaService _companias;

    public AseguradoServiceTests()
    {
        _service = new AseguradoService(_ctx.Db);
        _productores = new ProductorService(_ctx.Db);
        _polizas = new PolizaService(_ctx.Db);
        _companias = new CompaniaService(_ctx.Db);
    }

    [Fact] // Alta exitosa
    public async Task AltaAsegurado_conDocumento_seRegistra()
    {
        var productor = await _productores.AltaProductor("P", "p1", "clave");

        var asegurado = await _service.AltaAsegurado(productor.Id, "María López", TipoDocumento.Dni, "27333444");

        Assert.NotEqual(0, asegurado.Id);
    }

    [Fact] // Alta exitosa sin documento
    public async Task AltaAsegurado_sinDocumento_seRegistra()
    {
        var productor = await _productores.AltaProductor("P", "p1", "clave");

        var asegurado = await _service.AltaAsegurado(productor.Id, "María López");

        Assert.NotEqual(0, asegurado.Id);
        Assert.Null(asegurado.NroDocumento);
    }

    [Fact] // Documento duplicado
    public async Task AltaAsegurado_conDocumentoDuplicado_esRechazada()
    {
        var productor = await _productores.AltaProductor("P", "p1", "clave");
        await _service.AltaAsegurado(productor.Id, "María López", TipoDocumento.Dni, "27333444");

        await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => _service.AltaAsegurado(productor.Id, "Otra Persona", TipoDocumento.Dni, "27333444"));
    }

    [Fact] // Asegurado con pólizas en varias compañías y ramos (vista consolidada)
    public async Task ObtenerPolizasDeAsegurado_muestraPolizasDeDistintasCompaniasYRamos()
    {
        var productor = await _productores.AltaProductor("P", "p1", "clave");
        var asegurado = await _service.AltaAsegurado(productor.Id, "María López", TipoDocumento.Dni, "27333444");
        var companiaAutos = await _companias.AltaCompania("La Segunda", new[] { _ctx.RamoAutosId });
        var companiaVida = await _companias.AltaCompania("Zurich", new[] { _ctx.RamoVidaId });

        await _polizas.AltaPoliza(productor.Id, asegurado.Id, companiaAutos.Id, _ctx.RamoAutosId, "A-1",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 5000m);
        await _polizas.AltaPoliza(productor.Id, asegurado.Id, companiaVida.Id, _ctx.RamoVidaId, "V-1",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 3000m);

        var polizasDelAsegurado = await _service.ObtenerPolizasDeAsegurado(asegurado.Id);

        Assert.Equal(2, polizasDelAsegurado.Count);
        Assert.Contains(polizasDelAsegurado, p => p.Compania.Nombre == "La Segunda" && p.RamoId == _ctx.RamoAutosId);
        Assert.Contains(polizasDelAsegurado, p => p.Compania.Nombre == "Zurich" && p.RamoId == _ctx.RamoVidaId);
    }

    [Fact] // Búsqueda por documento
    public async Task Buscar_porDocumento_encuentraElAsegurado()
    {
        var productor = await _productores.AltaProductor("P", "p1", "clave");
        await _service.AltaAsegurado(productor.Id, "María López", TipoDocumento.Dni, "27333444");

        var resultado = await _service.Buscar(productor.Id, "27333444");

        Assert.Single(resultado);
    }

    [Fact] // Intento de eliminar asegurado con pólizas vigentes
    public async Task EliminarAsegurado_conPolizasVigentes_esRechazada()
    {
        var productor = await _productores.AltaProductor("P", "p1", "clave");
        var asegurado = await _service.AltaAsegurado(productor.Id, "María López", TipoDocumento.Dni, "27333444");
        var compania = await _companias.AltaCompania("La Segunda", new[] { _ctx.RamoAutosId });
        await _polizas.AltaPoliza(productor.Id, asegurado.Id, compania.Id, _ctx.RamoAutosId, "A-1",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 5000m);

        await Assert.ThrowsAsync<ReglaDeNegocioException>(() => _service.EliminarAsegurado(asegurado.Id));
    }

    [Fact] // Aislamiento de cartera entre productores (specs/productores)
    public async Task Buscar_noDevuelveAseguradosDeOtroProductor()
    {
        var productor1 = await _productores.AltaProductor("P1", "p1", "clave");
        var productor2 = await _productores.AltaProductor("P2", "p2", "clave");
        await _service.AltaAsegurado(productor1.Id, "Cliente de P1", TipoDocumento.Dni, "10000001");
        await _service.AltaAsegurado(productor2.Id, "Cliente de P2", TipoDocumento.Dni, "10000002");

        var resultado = await _service.Buscar(productor1.Id, "Cliente");

        Assert.Single(resultado);
        Assert.Equal("Cliente de P1", resultado[0].Nombre);
    }

    public void Dispose() => _ctx.Dispose();
}
