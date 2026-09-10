using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
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
    private int _companiaId;
    private readonly string _archivoXlsTemporal = Path.Combine(Path.GetTempPath(), $"seguros-test-{Guid.NewGuid():N}.xlsx");

    public ImportacionServiceTests()
    {
        _service = new ImportacionService(_ctx.Db);
        (_productorId, _companiaId) = Preparar().GetAwaiter().GetResult();
    }

    private async Task<(int productorId, int companiaId)> Preparar()
    {
        var productor = await new ProductorService(_ctx.Db).AltaProductor("P", "p1", "clave");
        var compania = await new CompaniaService(_ctx.Db).AltaCompania("La Segunda", new[] { _ctx.RamoAutosId });
        return (productor.Id, compania.Id);
    }

    private void CrearXlsDePrueba(bool conColumnaCompania = true)
    {
        using var workbook = new XLWorkbook();
        var hoja = workbook.Worksheets.Add("Polizas");
        var encabezados = new List<string> { "Documento", "Asegurado" };
        if (conColumnaCompania) encabezados.Add("Compania");
        encabezados.AddRange(new[] { "Ramo", "Nro Poliza", "Desde", "Hasta", "Prima" });

        for (var i = 0; i < encabezados.Count; i++) hoja.Cell(1, i + 1).Value = encabezados[i];

        var col = 1;
        hoja.Cell(2, col++).Value = "30111222";
        hoja.Cell(2, col++).Value = "Cliente Uno";
        if (conColumnaCompania) hoja.Cell(2, col++).Value = "La Segunda";
        hoja.Cell(2, col++).Value = "autos";
        hoja.Cell(2, col++).Value = "IMP-1";
        hoja.Cell(2, col++).Value = "01/01/2026";
        hoja.Cell(2, col++).Value = "01/01/2027";
        hoja.Cell(2, col).Value = "1500";

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

        Assert.Equal(CamposImportacion.NroDocumento, mapeo["Documento"]);
        Assert.Equal(CamposImportacion.NumeroPoliza, mapeo["Nro Poliza"]);
    }

    [Fact] // Vista previa editable + detección de posible duplicado
    public async Task GenerarVistaPrevia_marcaFilaDuplicadaCuandoYaExiste()
    {
        CrearXlsDePrueba();
        var filas = _service.LeerXls(_archivoXlsTemporal);
        var mapeo = _service.AutoDetectarMapeo(filas[0].Keys);

        await new AseguradoService(_ctx.Db).AltaAsegurado(_productorId, "Cliente Uno", TipoDocumento.Dni, "30111222");

        var preview = await _service.GenerarVistaPrevia(filas, mapeo, _productorId);

        Assert.True(preview.Single().EsPosibleDuplicado);
    }

    [Fact] // Archivo con columna de compañía
    public async Task GenerarVistaPrevia_usaLaColumnaDeCompaniaCuandoExiste()
    {
        CrearXlsDePrueba(conColumnaCompania: true);
        var filas = _service.LeerXls(_archivoXlsTemporal);
        var mapeo = _service.AutoDetectarMapeo(filas[0].Keys);

        var preview = await _service.GenerarVistaPrevia(filas, mapeo, _productorId);

        Assert.Equal("La Segunda", preview.Single().CompaniaNombre);
    }

    [Fact] // Archivo sin columna de compañía: se usa la compañía elegida para todo el archivo
    public async Task GenerarVistaPrevia_usaCompaniaPorDefectoCuandoNoHayColumna()
    {
        CrearXlsDePrueba(conColumnaCompania: false);
        var filas = _service.LeerXls(_archivoXlsTemporal);
        var mapeo = _service.AutoDetectarMapeo(filas[0].Keys);

        var preview = await _service.GenerarVistaPrevia(filas, mapeo, _productorId, companiaIdPorDefecto: _companiaId);

        Assert.Equal("La Segunda", preview.Single().CompaniaNombre);
        Assert.False(preview.Single().TieneError);
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
            NumeroFila = 1, NroDocumento = "30111222", NombreAsegurado = "Cliente Uno",
            CompaniaNombre = "La Segunda", RamoTexto = "autos", NumeroPoliza = "IMP-1",
            VigenciaDesdeTexto = "01/01/2026", VigenciaHastaTexto = "01/01/2027", PrimaTexto = "1500"
        };
        var itemConError = new ItemImportacion
        {
            NumeroFila = 2, ErrorDeteccion = "Falta el nombre del asegurado."
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

    [Fact] // Ramo con nomenclatura específica de una compañía: se agrega solo al catálogo
    public async Task ConfirmarImportacion_conRamoNuevo_loAgregaAlCatalogo()
    {
        var item = new ItemImportacion
        {
            NumeroFila = 1, NroDocumento = "30111222", NombreAsegurado = "Cliente Uno",
            CompaniaNombre = "La Segunda", RamoTexto = "Vida Colectivo Abierto", NumeroPoliza = "IMP-2",
            VigenciaDesdeTexto = "01/01/2026", VigenciaHastaTexto = "01/01/2027", PrimaTexto = "1500"
        };

        var resultado = await _service.ConfirmarImportacion(_productorId, new[] { (item, AccionImportacion.Importar) });

        Assert.Equal(1, resultado.Importados);
        Assert.True(await _ctx.Db.Ramos.AnyAsync(r => r.Nombre == "Vida Colectivo Abierto"));
    }

    [Fact] // Archivo sin columna de prima (ej. libro de movimientos): se importa con prima 0
    public async Task ConfirmarImportacion_sinColumnaDePrima_importaConPrimaCero()
    {
        var item = new ItemImportacion
        {
            NumeroFila = 1, NroDocumento = "30111222", NombreAsegurado = "Cliente Uno",
            CompaniaNombre = "La Segunda", RamoTexto = "autos", NumeroPoliza = "IMP-9",
            VigenciaDesdeTexto = "01/01/2026", VigenciaHastaTexto = "01/01/2027", PrimaTexto = null
        };

        var resultado = await _service.ConfirmarImportacion(_productorId, new[] { (item, AccionImportacion.Importar) });

        Assert.Equal(1, resultado.Importados);
        var poliza = await _ctx.Db.Polizas.SingleAsync(p => p.Numero == "IMP-9");
        Assert.Equal(0m, poliza.Prima);
    }

    [Fact] // Estado de la póliza según el archivo (ej. libro de movimientos con "SOLICITUD ANULACION")
    public async Task ConfirmarImportacion_conEstadoAnulada_creaLaPolizaComoAnulada()
    {
        var item = new ItemImportacion
        {
            NumeroFila = 1, NroDocumento = "30111222", NombreAsegurado = "Cliente Uno",
            CompaniaNombre = "La Segunda", RamoTexto = "autos", NumeroPoliza = "IMP-10",
            VigenciaDesdeTexto = "01/01/2026", VigenciaHastaTexto = "01/01/2027", PrimaTexto = "1000",
            EstadoTexto = "SOLICITUD ANULACION"
        };

        await _service.ConfirmarImportacion(_productorId, new[] { (item, AccionImportacion.Importar) });

        var poliza = await _ctx.Db.Polizas.SingleAsync(p => p.Numero == "IMP-10");
        Assert.Equal(Seguros.Domain.Enums.EstadoPoliza.Anulada, poliza.Estado);
    }

    [Fact] // Asegurado sin documento: se importa igual y se detecta posible duplicado por nombre
    public async Task GenerarVistaPrevia_sinDocumento_detectaDuplicadoPorNombre()
    {
        await new AseguradoService(_ctx.Db).AltaAsegurado(_productorId, "Cliente Sin Doc");

        var filas = new List<Dictionary<string, string>>
        {
            new()
            {
                ["Asegurado"] = "Cliente Sin Doc", ["Compania"] = "La Segunda", ["Ramo"] = "autos",
                ["Nro Poliza"] = "IMP-3", ["Desde"] = "01/01/2026", ["Hasta"] = "01/01/2027", ["Prima"] = "1500"
            }
        };
        var mapeo = _service.AutoDetectarMapeo(filas[0].Keys);

        var preview = await _service.GenerarVistaPrevia(filas, mapeo, _productorId);

        var item = Assert.Single(preview);
        Assert.False(item.TieneError);
        Assert.True(item.EsPosibleDuplicado);
    }

    [Fact] // Archivo XLS con filas de título antes del encabezado real (ej. cartera_vigente)
    public void LeerXls_conFilaDeEncabezadoDistintaDeUno_saltaLasFilasDeTitulo()
    {
        using (var workbook = new XLWorkbook())
        {
            var hoja = workbook.Worksheets.Add("Cartera");
            hoja.Cell(1, 1).Value = "CARTERA VIGENTE AL 04/09/2026";
            hoja.Cell(2, 1).Value = "PAS: 6380";
            hoja.Cell(3, 1).Value = "Documento";
            hoja.Cell(3, 2).Value = "Asegurado";
            hoja.Cell(4, 1).Value = "30111222";
            hoja.Cell(4, 2).Value = "Cliente Uno";
            workbook.SaveAs(_archivoXlsTemporal);
        }

        var filas = _service.LeerXls(_archivoXlsTemporal, filaEncabezado: 3);

        Assert.Single(filas);
        Assert.Equal("30111222", filas[0]["Documento"]);
    }

    [Fact] // Carga de archivo CSV (libro de movimientos)
    public void LeerCsv_delimitadoPorPuntoYComa_devuelveLasFilas()
    {
        var rutaCsv = Path.Combine(Path.GetTempPath(), $"seguros-test-{Guid.NewGuid():N}.csv");
        File.WriteAllLines(rutaCsv, new[]
        {
            "Documento;Asegurado;Nro Poliza",
            "30111222;Cliente Uno;IMP-1"
        });

        try
        {
            var filas = _service.LeerCsv(rutaCsv);

            Assert.Single(filas);
            Assert.Equal("Cliente Uno", filas[0]["Asegurado"]);
        }
        finally
        {
            File.Delete(rutaCsv);
        }
    }

    [Fact] // Deduplicar un historial de movimientos: se queda con la fila más reciente por póliza
    public void DeduplicarPorMasReciente_seQuedaConLaFilaMasNuevaPorPoliza()
    {
        var filas = new List<Dictionary<string, string>>
        {
            new() { ["Poliza"] = "P-1", ["Desde"] = "01/01/2026", ["Prima"] = "1000" },
            new() { ["Poliza"] = "P-1", ["Desde"] = "01/06/2026", ["Prima"] = "1200" }, // más reciente
            new() { ["Poliza"] = "P-2", ["Desde"] = "01/03/2026", ["Prima"] = "500" },
        };

        var resultado = _service.DeduplicarPorMasReciente(filas, "Poliza", "Desde");

        Assert.Equal(2, resultado.Count);
        Assert.Equal("1200", resultado.Single(f => f["Poliza"] == "P-1")["Prima"]);
    }

    public void Dispose()
    {
        _ctx.Dispose();
        if (File.Exists(_archivoXlsTemporal)) File.Delete(_archivoXlsTemporal);
    }
}
