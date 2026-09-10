## 1. Lectura de archivos

- [x] 1.1 Implementar `LectorArchivos.LeerCsv` con detección automática de delimitador (`;` o `,`) y manejo de campos entre comillas, verificado con un archivo de prueba
- [x] 1.2 Corregir `LeerXls` para que la fila de encabezado se cuente como en Excel (incluyendo filas en blanco) en vez de indexar sobre las filas con contenido, verificado con un archivo con filas de título antes del encabezado real

## 2. Campos y validación de importación

- [x] 2.1 Agregar el campo mapeable `Estado` (Vigente/Anulada/Renovada/No vigente) a `CamposImportacion`/`ItemImportacion`, y usarlo al crear o actualizar la póliza en `ImportarFila`, verificando que sin mapear sigue asumiendo Vigente
- [x] 2.2 Hacer la prima opcional en la validación: sin columna mapeada se importa con prima 0; con columna mapeada pero valor inválido, se sigue rechazando la fila, verificado con tests para ambos casos
- [x] 2.3 Reconocer prefijos de moneda adicionales (`U$S`, `USD`, `AR$`) en `ParseoImportacion.TryParseMonto`, verificado con un valor real (`U$S250.49`)

## 3. Deduplicación de historiales

- [x] 3.1 Implementar `ImportacionService.DeduplicarPorMasReciente` (agrupa por una columna clave y se queda con la fila de fecha más reciente de una columna indicada), verificado con un test de historial con varias filas por póliza

## 4. Interfaz de Importación

- [x] 4.1 Actualizar `ImportacionView` para elegir también archivos CSV, indicar la fila de encabezado y poder releer el archivo con un valor distinto
- [x] 4.2 Agregar la opción "quedarme con la fila más reciente por póliza" en la pantalla, aplicada antes de generar la vista previa cuando el productor la marca

## 5. Verificación con datos reales

- [x] 5.1 Usar el importador (a través de los mismos servicios, sin atajos) para cargar a la base real las carteras de Mapfre, La Holando y LIDERAR, verificando las cantidades finales de compañías, asegurados y pólizas por estado
- [x] 5.2 Ejecutar `openspec validate ampliar-importacion-multiformato --strict` y correr la suite completa de tests antes de archivar la change
