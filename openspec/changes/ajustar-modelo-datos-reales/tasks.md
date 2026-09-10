## 1. Catálogo de ramos

- [x] 1.1 Crear la entidad `Ramo` (Id, Nombre) en `Seguros.Domain` y eliminar el enum `Ramo` existente
- [x] 1.2 Cambiar `CompaniaRamo` y `Poliza` para referenciar `RamoId` (FK a `Ramo`) en vez del enum, y verificar que el proyecto compila
- [x] 1.3 Implementar `RamoService` (alta con validación de nombre duplicado sin distinguir mayúsculas/minúsculas, listado) y verificar con un test el rechazo de un nombre duplicado

## 2. Documento del asegurado

- [x] 2.1 Agregar el enum `TipoDocumento` (Dni, Cuit, Le, Otro) en `Seguros.Domain`
- [x] 2.2 Reemplazar `Asegurado.Documento` por `TipoDocumento?` y `NroDocumento` (nullable) y actualizar el índice único para que siga funcionando con documento ausente
- [x] 2.3 Actualizar `AseguradoService.AltaAsegurado` para aceptar documento opcional y verificar con tests: alta sin documento, alta con documento, documento duplicado (mismo tipo+número para el mismo productor)

## 3. Campos nuevos de póliza

- [x] 3.1 Agregar `Producto`, `Premio` y `Cobertura` (todos opcionales) a `Poliza` y actualizar `PolizaService.AltaPoliza`/`EditarPoliza` para aceptarlos, verificando con un test que se guardan correctamente
- [x] 3.2 Agregar `DescripcionRiesgo` (opcional) a `Poliza` para el caso de una póliza sin flota, y verificar con un test que se puede guardar sin necesidad de crear unidades de flota
- [x] 3.3 Agregar `EstadoPoliza.NoVigente` y el método `PolizaService.MarcarComoNoVigente`, verificando con un test que la póliza deja de contarse como vigente sin quedar como anulada

## 4. Migración de base de datos

- [x] 4.1 Eliminar la migración `InicialEsquema` existente (`dotnet ef migrations remove`) y generar una nueva migración inicial con el esquema actualizado
- [x] 4.2 Aplicar la migración sobre una base de datos local limpia y verificar que se crea sin errores
- [x] 4.3 Sembrar el catálogo de `Ramo` (en el primer arranque de la aplicación) con los nombres observados en los 3 archivos reales analizados: Autos, Motos, Vida, Vida Colectivo, Vida Individual, Vida Obligatorio, Incendio, Combinado Familiar, Responsabilidad Civil, Accidentes Personales, Integral de Comercio, Caución, Transporte/Cascos

## 5. Importación: compañía por columna o por archivo

- [x] 5.1 Extender `ImportacionService` para aceptar el nombre de la compañía como un campo mapeable por fila (`CompaniaNombre`, ya existe en `CamposImportacion`) o como un valor único aplicado a todas las filas del lote
- [x] 5.2 Actualizar `ImportacionView`: si el productor no mapea ninguna columna a "Compañía", pedirle elegir una compañía del catálogo antes de generar la vista previa, y aplicarla a todas las filas
- [x] 5.3 Ajustar el parseo de moneda para aceptar tanto el formato `es-AR` (punto de miles, coma decimal) como el formato con coma de miles y punto decimal visto en archivos reales (ej. `"$199,036.44"`), verificando ambos casos con tests
- [x] 5.4 Ajustar el parseo de fechas para aceptar años de 2 dígitos (ej. `02/01/26`) además del formato de 4 dígitos ya soportado, verificando ambos casos con tests

## 6. Interfaz: catálogo de ramos en vez de enum fijo

- [x] 6.1 Actualizar `CompaniasView`, `PolizasView` y `ComisionesView` para poblar el selector de ramo desde `RamoService.Listar()` en vez de `Enum.GetValues<Ramo>()`
- [x] 6.2 Agregar en alguna pantalla (o una nueva pestaña simple) la posibilidad de dar de alta un ramo nuevo en el catálogo desde la interfaz

## 7. Actualización de tests existentes

- [x] 7.1 Actualizar todos los tests que usaban el enum `Ramo` o el campo `Documento` obligatorio para usar el nuevo modelo (catálogo de ramos, documento opcional), verificando que la suite completa sigue pasando

## 8. Verificación integral

- [x] 8.1 Compilar toda la solución y correr la suite completa de tests, verificando 0 errores y 0 tests fallidos
- [x] 8.2 Probar manualmente (o con un flujo de test de integración) una importación de un archivo sin columna de compañía (selección manual) y otra con columna de compañía, verificando que ambas rutas cargan correctamente
- [x] 8.3 Ejecutar `openspec validate ajustar-modelo-datos-reales --strict` y verificar que la change sigue siendo válida antes de archivarla
