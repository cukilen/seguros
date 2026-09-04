using ClosedXML.Excel;
using Seguros.Data.Importacion;
using Seguros.Data.Services;
using Seguros.Domain.Enums;
using Xunit;

namespace Seguros.Tests;

// specs/importacion-datos/spec.md
public class ImportacionServiceTests : IDisposable
{
    private readonly SqliteTestContext _ctx = new();
    private readonly ImportacionService _service;
    private int _productorId;
    private readonly string _archivoXlsTemporal = Path.Combine(Path.GetTempPath(), $"seguros-test-{Guid.NewGuid():N}.xlsx");

    public ImportacionServiceTests()
    {
        _service = new ImportacionService(_ctx.Db);
        _productorId = Preparar().GetAwaiter().GetResult();
    }

    private async Task<int> Preparar()
    {
        var productor = await new ProductorService(_ctx.Db).AltaProductor("P", "p1", "clave");
        await new CompaniaService(_ctx.Db).AltaCompania("La Segunda", new[] { Ramo.Autos });
        return productor.Id;
    }

    private void CrearXlsDePrueba()
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Polizas");
        string[] encabezados = { "Documento", "Asegurado", "Compania", "Ramo", "Nro Poliza", "Desde", "Hasta", "Prima" };
        for (var i = 0; i < encabezados.Length; i++) hoja.Cell(1, i + 1).Value = encabezados[i];

        hoja.Cell(2, 1).Value = "30111222"; hoja.Cell(2, 2).Value = "Cliente Uno";
        hoja.Cell(2, 3).Value = "La Segunda"; hoja.Cell(2, 4).Value = "autos";
        hoja.Cell(2, 5).Value = "IMP-1"; hoja.Cell(2, 6).Value = "01/01/2026";
        hoja.Cell(2, 7).Value = "01/01/2027"; hoja.Cell(2, 8).Value = "1500";

        workbook.SaveAs(_archivoXlsTemporal);
    }

    [Fact] // Carga de archivo XLS válido
    public void LeerXls_devuelveLasFilasDelArchivo()
    {
        CrearXlsDePrueba();

        var filas = _service.LeerXls(_archivoXlsTemporal);

        Assert.Single(filas);
        Assert.Equal("30111222", filas[0]["Documento"]);
    }

    [Fact] // Mapeo manual de columnas + detección automática
    public void AutoDetectarMapeo_reconoceEncabezadosComunes()
    {
        var mapeo = _service.AutoDetectarMapeo(new[] { "Documento", "Asegurado", "Compania", "Ramo", "Nro Poliza", "Desde", "Hasta", "Prima" });

        Assert.Equal(CamposImportacion.Documento, mapeo["Documento"]);
        Assert.Equal(CamposImportacion.NumeroPoliza, mapeo["Nro Poliza"]);
    }

    [Fact] // Vista previa editable + detección de posible duplicado
    public async Task GenerarVistaPrevia_marcaFilaDuplicadaCuandoYaExiste()
    {
        CrearXlsDePrueba();
        var filas = _service.LeerXls(_archivoXlsTemporal);
        var mapeo = _service.AutoDetectarMapeo(filas[0].Keys);

        await new AseguradoService(_ctx.Db).AltaAsegurado(_productorId, "Cliente Uno", "30111222");

        var preview = await _service.GenerarVistaPrevia(filas, mapeo, _productorId);

        Assert.True(preview.Single().EsPosibleDuplicado);
    }

    [Fact] // PDF sin datos reconocibles
    public void InterpretarLineasTabulares_sinEstructuraDeColumnas_devuelveNull()
    {
        var lineas = new[] { "Esto es un párrafo suelto sin columnas.", "Otra línea de texto libre." };

        var resultado = LectorArchivos.InterpretarLineasTabulares(lineas);

        Assert.Null(resultado);
    }

    [Fact] // Extracción exitosa desde PDF (misma interpretación tabular que XLS)
    public void InterpretarLineasTabulares_conColumnasSeparadasPorEspacios_lasReconoce()
    {
        var lineas = new[]
        {
            "Documento   Asegurado    NroPoliza",
            "30111222    Cliente Uno  IMP-1"
        };

        var resultado = LectorArchivos.InterpretarLineasTabulares(lineas);

        Assert.NotNull(resultado);
        Assert.Equal("30111222", resultado![0]["Documento"]);
    }

    [Fact] // Reporte de resultado de importación con errores parciales
    public async Task ConfirmarImportacion_importaFilasValidasYReportaErrores()
    {
        var itemValido = new ItemImportacion
        {
            NumeroFila = 1, Documento = "30111222", NombreAsegurado = "Cliente Uno",
            CompaniaNombre = "La Segunda", RamoTexto = "autos", NumeroPoliza = "IMP-1",
            VigenciaDesdeTexto = "01/01/2026", VigenciaHastaTexto = "01/01/2027", PrimaTexto = "1500"
        };
        var itemConError = new ItemImportacion
        {
            NumeroFila = 2, ErrorDeteccion = "Falta el documento del asegurado."
        };

        var resultado = await _service.ConfirmarImportacion(_productorId, new[]
        {
            (itemValido, AccionImportacion.Importar),
            (itemConError, AccionImportacion.Importar)
        });

        Assert.Equal(1, resultado.Importados);
        Assert.Equal(1, resultado.ConError);
        Assert.Single(resultado.Errores);
    }

    public void Dispose()
    {
        _ctx.Dispose();
        if (File.Exists(_archivoXlsTemporal)) File.Delete(_archivoXlsTemporal);
    }
}
