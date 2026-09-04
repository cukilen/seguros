using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Xunit;

namespace Seguros.Tests;

// tasks.md 13.1: flujo de punta a punta que combina todas las capacidades
public class FlujoIntegralTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();

    [Fact]
    public async Task FlujoCompleto_productorHastaLiquidacionDeComision()
    {
        var productores = new ProductorService(_ctx.Db);
        var companias = new CompaniaService(_ctx.Db);
        var asegurados = new AseguradoService(_ctx.Db);
        var polizas = new PolizaService(_ctx.Db);
        var flotas = new FlotaService(_ctx.Db);
        var endosos = new EndosoService(_ctx.Db);
        var vencimientos = new VencimientoService(_ctx.Db);
        var comisiones = new ComisionService(_ctx.Db);

        // 1. Alta de productor
        var productor = await productores.AltaProductor("Juan Pérez", "juan", "clave123");
        Assert.NotNull(await productores.Login("juan", "clave123"));

        // 2. Alta de compañía
        var compania = await companias.AltaCompania("La Segunda", new[] { Ramo.Autos });
        await comisiones.ConfigurarComision(compania.Id, Ramo.Autos, 12m);

        // 3. Alta de asegurado
        var asegurado = await asegurados.AltaAsegurado(productor.Id, "María López", "27333444");

        // 4. Alta de póliza de flota
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var poliza = await polizas.AltaPoliza(productor.Id, asegurado.Id, compania.Id, Ramo.Autos, "P-100",
            hoy.AddDays(-350), hoy.AddDays(10), 10000m, esFlota: true);

        await flotas.MarcarComoFlota(poliza.Id);
        await flotas.AltaUnidad(poliza.Id, "AB123CD", "Ford", "Transit", "Carga");

        var polizasDelAsegurado = await asegurados.ObtenerPolizasDeAsegurado(asegurado.Id);
        Assert.Single(polizasDelAsegurado);

        // 5. Endoso sobre la póliza
        var endoso = await endosos.AltaEndoso(poliza.Id, hoy, "Cambio de domicilio", "Nuevo domicilio del asegurado");
        Assert.Single(await endosos.HistorialEndosos(poliza.Id));

        // 6. La póliza aparece en alertas de vencimiento (vence en 10 días, ventana de 30)
        var alertas = await vencimientos.ListarProximasAVencer(diasVentana: 30, productorId: productor.Id);
        Assert.Contains(alertas, p => p.Id == poliza.Id);

        // 7. Liquidación de comisión del período que incluye la emisión de la póliza
        var liquidacion = await comisiones.GenerarLiquidacion(productor.Id, hoy.AddDays(-360), hoy);
        Assert.Equal(1200m, liquidacion.MontoTotal); // 12% de 10000

        var historial = await comisiones.HistorialLiquidaciones(productor.Id);
        Assert.Single(historial);
    }

    public void Dispose() => _ctx.Dispose();
}
