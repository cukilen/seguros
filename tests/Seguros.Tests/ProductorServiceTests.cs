using Seguros.Data.Services;
using Seguros.Domain.Exceptions;
using Xunit;

namespace Seguros.Tests;

// specs/productores/spec.md
public class ProductorServiceTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();
    private readonly ProductorService _service;

    public ProductorServiceTests() => _service = new ProductorService(_ctx.Db);

    [Fact] // Alta de productor exitosa
    public async Task AltaProductor_conDatosValidos_permiteLoguearse()
    {
        await _service.AltaProductor("Juan Pérez", "juan", "clave123");

        var logueado = await _service.Login("juan", "clave123");

        Assert.NotNull(logueado);
        Assert.Equal("Juan Pérez", logueado!.Nombre);
    }

    [Fact] // Credenciales inválidas
    public async Task Login_conPasswordIncorrecta_devuelveNull()
    {
        await _service.AltaProductor("Juan Pérez", "juan", "clave123");

        var logueado = await _service.Login("juan", "otraClave");

        Assert.Null(logueado);
    }

    [Fact] // Aislamiento de cartera entre productores (login habilita/deshabilita el acceso)
    public async Task Desactivacion_impideLogin_peroConservaHistorial()
    {
        var productor = await _service.AltaProductor("Ana Gómez", "ana", "clave123");

        await _service.DesactivarProductor(productor.Id);
        var logueado = await _service.Login("ana", "clave123");

        Assert.Null(logueado);
        Assert.False((await _ctx.Db.Productores.FindAsync(productor.Id))!.Activo);
    }

    [Fact]
    public async Task AltaProductor_conUsuarioDuplicado_esRechazada()
    {
        await _service.AltaProductor("Juan Pérez", "juan", "clave123");

        await Assert.ThrowsAsync<ReglaDeNegocioException>(
            () => _service.AltaProductor("Otro Juan", "juan", "otraClave"));
    }

    [Fact] // El usuario de acceso no distingue mayúsculas/minúsculas
    public async Task Login_noDistingueMayusculasEnElUsuario()
    {
        await _service.AltaProductor("Juan Pérez", "JuanTest", "clave123");

        var logueado = await _service.Login("juantest", "clave123");

        Assert.NotNull(logueado);
    }

    public void Dispose() => _ctx.Dispose();
}
