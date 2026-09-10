## Why

Al importar los 3 archivos reales del productor (dos exports de compañías y el libro de movimientos) aparecieron limitaciones concretas del importador: no leía CSV, interpretaba mal la fila de encabezado en archivos con filas de título arriba, siempre creaba las pólizas como "Vigente" sin importar el estado real del archivo, exigía una prima que algunas fuentes no traen, y algunos montos con prefijo de moneda (`U$S`) no se reconocían. Se resolvieron estas limitaciones para poder cargar esos archivos, y esta change las documenta formalmente.

## What Changes

- El importador ahora acepta archivos **CSV** (además de XLS y PDF), detectando el delimitador (`;` o `,`) automáticamente.
- La **fila de encabezado** para XLS/CSV es configurable y se cuenta como en Excel (número de fila real, incluyendo filas en blanco), para archivos con filas de título antes del encabezado real.
- Se agrega el campo mapeable **Estado** de la póliza (Vigente/Anulada/Renovada/No vigente); si no se mapea, se sigue asumiendo Vigente como antes. **BREAKING** en el sentido de que una importación con esa columna mapeada ahora puede crear pólizas en un estado distinto de Vigente, cosa que antes no pasaba nunca.
- La **prima pasa a ser opcional**: si el archivo no trae una columna de prima, la póliza se importa con prima 0 en lugar de rechazar la fila completa. Si la columna está mapeada pero el valor es inválido, se sigue rechazando esa fila.
- Se reconocen más **prefijos de moneda** en los montos (`U$S`, `USD`, `AR$`, además de `$`).
- Se agrega una operación de **deduplicación por fila más reciente**: para archivos que traen varias filas por póliza (historial de movimientos), permite quedarse con una sola fila por póliza según la fecha más reciente de una columna indicada, antes de generar la vista previa.

## Capabilities

### Modified Capabilities
- `importacion-datos`: se agregan las capacidades de carga CSV, fila de encabezado configurable, campo Estado, prima opcional, y deduplicación por fila más reciente.

## Impact

- Cambios en `Seguros.Data/Importacion/` (`LectorArchivos`, `ImportacionService`, `ItemImportacion`, `MapeoColumnas`, `ParseoImportacion`) y en `Seguros.App/Vistas/ImportacionView`.
- Ya se usó esta versión del importador para cargar a la base real las carteras de Mapfre, La Holando y LIDERAR (1531 pólizas, 436 asegurados) — no es un cambio solo teórico, ya está validado con datos reales.
