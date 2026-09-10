## Why

El modelo de datos original se diseñó con supuestos razonables pero no verificados contra la realidad. Al analizar 3 archivos reales (dos exports de compañías y el libro de movimientos del productor) aparecieron diferencias de fondo: el documento del asegurado no siempre está disponible o tiene distinto tipo (DNI/CUIT/LE), el "ramo" tiene muchas más variantes de las que contemplaba un enum cerrado, la compañía no siempre viene como columna en el archivo, y una póliza distingue prima neta de premio total. Conviene ajustar el modelo ahora, antes de que haya datos reales cargados, para que la importación funcione con los archivos que el productor realmente recibe.

## What Changes

- El documento del asegurado pasa a ser opcional y se separa en tipo (DNI/CUIT/LE/Otro) + número, en vez de un único campo de texto obligatorio. **BREAKING** para quien ya haya cargado asegurados con el modelo anterior (no aplica todavía: no hay datos reales cargados).
- El ramo deja de ser un enum cerrado de 6 valores y pasa a ser un catálogo abierto (tabla), para admitir los ~15+ nombres de ramo/sección distintos observados entre las 3 fuentes reales.
- La póliza suma: `Producto` (nombre comercial del plan), `Premio` (total, distinto de la prima neta) y `Cobertura` (nivel de cobertura, relevante en autos: A/B/B1/C/CL/CP).
- El estado de póliza suma `NoVigente` (vencida sin gestionar), distinto de `Anulada`.
- Cuando una póliza no es una flota explícita, se agrega una descripción de riesgo simple en texto libre (por ejemplo, el vehículo asegurado) en vez de forzar la estructura de unidades de flota para un solo ítem.
- La importación deja de exigir la compañía como columna en cada fila: si el archivo la trae (como el libro de movimientos) se usa esa columna; si no (como ambos exports de compañía), se selecciona una sola vez para todo el archivo importado.

## Capabilities

### New Capabilities
- `ramos`: Catálogo abierto de ramos/secciones de seguro, reemplazando el enum cerrado anterior.

### Modified Capabilities
- `asegurados`: El documento pasa a ser opcional y se separa en tipo + número.
- `polizas`: Se agregan Producto, Premio, Cobertura y el estado NoVigente; se agrega descripción de riesgo simple para pólizas sin flota.
- `importacion-datos`: La compañía se toma de una columna del archivo cuando existe, o se selecciona una vez por archivo cuando no.

## Impact

- Cambios de esquema en `Seguros.Domain`/`Seguros.Data`: nueva entidad `Ramo` (tabla), cambios en `Asegurado` (documento) y `Poliza` (nuevos campos), nueva migración de EF Core.
- No hay datos reales en producción todavía (única base existente era de pruebas y ya se descartó), por lo que la migración de datos no es un riesgo real en este momento.
- Se ajusta `ImportacionService`/`ImportacionView` para el nuevo flujo de selección de compañía y el catálogo abierto de ramos.
