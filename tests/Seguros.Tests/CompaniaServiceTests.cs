using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Seguros.Domain.Exceptions;
using Xunit;

namespace Seguros.Tests;

// specs/companias/spec.md
public class CompaniaServiceTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();
    private readonly CompaniaService _service;

    public CompaniaServiceTests() => _service = new CompaniaService(_ctx.Db);

    [Fact] // Alta exitosa
    public async Task AltaCompania_conNombreYRamo_quedaDisponibleParaPolizas()
    {
        var compania = await _service.AltaCompania("La Segunda", new[] { _ctx.RamoAutosId, _ctx.RamoVidaId });

        var listado = await _service.ListarCompanias();

        Assert.Contains(listado, c => c.Id == compania.Id && c.RamosOperados.Count == 2);
    }

    [Fact] // Falta el nombre de la compañía
    public async Task AltaCompania_sinNombre_esRechazada()
    {
        await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => _service.AltaCompania("", new[] { _ctx.RamoAutosId }));
    }

    [Fact] // Actualización de datos de contacto
    public async Task EditarDatosContacto_actualizaTelefonoYEmail()
    {
        var compania = await _service.AltaCompania("Sancor", new[] { _ctx.RamoAutosId });

        await _service.EditarDatosContacto(compania.Id, "011-1234", "contacto@sancor.com");

        var actualizada = (await _service.ListarCompanias()).Single(c => c.Id == compania.Id);
        Assert.Equal("011-1234", actualizada.Telefono);
    }

    [Fact] // Intento de eliminar compañía con pólizas vigentes
    public async Task EliminarCompania_conPolizasAsociadas_esRechazada()
    {
        var compania = await _service.AltaCompania("Mercantil Andina", new[] { _ctx.RamoAutosId });
        await CrearPolizaDePrueba(compania.Id);

        await Assert.ThrowsAsync<ReglaDeNegocioException>(() => _service.EliminarCompania(compania.Id));
    }

    [Fact] // Inactivación de compañía sin pólizas
    public async Task InactivarCompania_sinPolizas_quedaInactiva()
    {
        var compania = await _service.AltaCompania("Zurich", new[] { _ctx.RamoVidaId });

        await _service.InactivarCompania(compania.Id);

        var actualizada = (await _service.ListarCompanias()).Single(c => c.Id == compania.Id);
        Assert.False(actualizada.Activa);
    }

    private async Task CrearPolizaDePrueba(int companiaId)
    {
        var productor = (await new ProductorService(_ctx.Db).AltaProductor("P", "p1", "clave")).Id;
        var asegurado = (await new AseguradoService(_ctx.Db).AltaAsegurado(productor, "Cliente", TipoDocumento.Dni, "20111222")).Id;
        await new PolizaService(_ctx.Db).AltaPoliza(productor, asegurado, companiaId, _ctx.RamoAutosId, "P-1",
            DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddYears(1)), 1000m);
    }

    public void Dispose() => _ctx.Dispose();
}
