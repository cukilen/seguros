using Seguros.Data.Importacion;
using Xunit;

namespace Seguros.Tests;

// tasks.md 5.3/5.4: la importación acepta más de un formato de fecha/moneda
public class ParseoImportacionTests
{
    [Theory]
    [InlineData("01/01/2026", 2026)] // es-AR, año de 4 dígitos
    [InlineData("02/01/26", 2026)]   // año de 2 dígitos (libro de movimientos)
    public void TryParseFecha_aceptaFormatosReales(string texto, int anioEsperado)
    {
        var ok = ParseoImportacion.TryParseFecha(texto, out var fecha);

        Assert.True(ok);
        Assert.Equal(anioEsperado, fecha.Year);
    }

    [Theory]
    [InlineData("20621,3", 20621.3)]          // es-AR: coma decimal (archivo cartera vigente)
    [InlineData("$199,036.44", 199036.44)]    // coma de miles, punto decimal (archivo poliza-en-cartera)
    [InlineData("34,965,000", 34965000)]      // coma de miles sin parte decimal
    public void TryParseMonto_aceptaFormatosReales(string texto, decimal montoEsperado)
    {
        var ok = ParseoImportacion.TryParseMonto(texto, out var monto);

        Assert.True(ok);
        Assert.Equal(montoEsperado, monto);
    }

    [Fact]
    public void TryParseMonto_conTextoInvalido_devuelveFalse()
    {
        Assert.False(ParseoImportacion.TryParseMonto("no es un número", out _));
    }
}
