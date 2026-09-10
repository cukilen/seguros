## Context

Ver `proposal.md` - Why para la motivación. A nivel técnico, el modelo actual (implementado en la change `gestion-integral-seguros`, ya archivada) tiene: `Ramo` como enum de C# con 6 valores fijos, `Asegurado.Documento` como string obligatorio único, `Poliza` sin `Producto`/`Premio`/`Cobertura`/descripción de riesgo simple, y `EstadoPoliza` sin el valor "no vigente". No hay datos reales cargados todavía (la única base existente era de pruebas y ya se descartó), lo que simplifica la migración.

## Goals / Non-Goals

**Goals:**
- Que el catálogo de ramos admita cualquier nombre que traigan las compañías reales, sin recompilar código para agregar uno nuevo.
- Que el alta de asegurado no bloquee por falta de documento cuando la fuente de datos no lo tiene.
- Que la importación de un archivo sin columna de compañía siga funcionando (seleccionando la compañía una vez), y que un archivo con columna de compañía (como el libro de movimientos) la use directamente.

**Non-Goals:**
- No se resuelve en este cambio la deduplicación de asegurados que aparecen en más de un archivo sin documento (por ejemplo, mismo nombre importado desde dos archivos distintos sin poder cruzarlos). Queda como limitación conocida.
- No se generaliza `UnidadFlota` a un concepto de "ítems de riesgo" para toda póliza (se evaluó y se descartó por ahora - ver proposal.md, pregunta resuelta con el usuario). Una póliza sin flota usa la nueva descripción de riesgo en texto libre; la estructura de unidades sigue siendo exclusiva de flotas.
- No se migran datos reales existentes: no hay ninguno cargado todavía.

## Decisions

**1. `Ramo` pasa de enum de C# a entidad/tabla**
Alternativa considerada: ampliar el enum con más valores fijos. Se descarta porque entre las 3 fuentes reales ya aparecieron ~15 nombres distintos de ramo/sección, y cada compañía nueva puede traer otro más - un enum cerrado obliga a recompilar cada vez. Se reemplaza `Poliza.Ramo` y `CompaniaRamo.Ramo` (antes `Ramo` enum) por `RamoId` (FK a la nueva tabla `Ramo`). Se precarga el catálogo con los nombres ya observados en los 3 archivos analizados (Autos, Motos, Vida, Vida Colectivo, Vida Individual, Vida Obligatorio, Incendio, Combinado Familiar, RC/Responsabilidad Civil, Accidentes Personales, Integral de Comercio, Caución, Transporte/Cascos) para no arrancar de un catálogo vacío.

**2. `Asegurado.Documento` se separa en `TipoDocumento` (enum: Dni, Cuit, Le, Otro) + `NroDocumento` (string, nullable)**
Alternativa considerada: mantener un solo campo string libre tipo "DNI 12345678". Se descarta porque mezclar tipo y número en un string dificulta buscar/comparar, y porque el libro de movimientos sí distingue DNI/CUIT/LE como datos separados. `NroDocumento` es nullable porque el archivo 2 (poliza-en-cartera) no lo trae. El índice único (`ProductorId`, `TipoDocumento`, `NroDocumento`) sigue funcionando igual en SQLite: las filas con `NroDocumento` nulo no colisionan entre sí (SQLite no aplica unicidad sobre valores NULL), permitiendo múltiples asegurados sin documento para el mismo productor.

**3. `Poliza` suma `Producto` (string, nullable), `Premio` (decimal, nullable), `Cobertura` (string, nullable) y `DescripcionRiesgo` (string, nullable)**
Todos opcionales porque ninguno está garantizado en todas las fuentes (por ejemplo, el archivo 1 no distingue premio de prima; el archivo 2 sí). `DescripcionRiesgo` se usa cuando la póliza no es de flota (ver Non-Goals); cuando sí es flota, la descripción vive en `UnidadFlota` como ya estaba.

**4. `EstadoPoliza` suma el valor `NoVigente`**
Se agrega junto a los ya existentes (`Vigente`, `Anulada`, `Renovada`). Es una transición manual (el productor la marca), no automática por fecha, para no anular pólizas prematuramente sin que el productor confirme que efectivamente no se gestionó.

**5. Selección de compañía en la importación: por columna si existe, si no, una vez por archivo**
La UI de importación ya tiene un paso de mapeo de columnas (agregado en la change anterior). Se extiende ese mismo paso: si el productor mapea una columna al campo "Compañía", se usa por fila; si no mapea ninguna, antes de generar la vista previa se le pide elegir una compañía del catálogo existente, y se aplica a todas las filas.

**6. Regenerar la migración inicial en lugar de agregar una migración incremental**
Como no hay datos reales cargados, se elimina la migración `InicialEsquema` existente y se genera una nueva desde cero con el esquema completo ya actualizado, en vez de encadenar una migración incremental compleja (que tendría que convertir una columna enum a una FK sin datos que preservar). Si en el futuro ya hay datos reales cargados antes de un cambio de esquema así, esta estrategia de "recrear la migración inicial" ya no sería válida y habría que escribir una migración incremental real.

## Risks / Trade-offs

- **[Riesgo]** Asegurados sin documento importados desde distintos archivos pueden duplicarse (mismo nombre, sin forma de cruzarlos). → **Mitigación**: aceptado como limitación conocida (ver Non-Goals); la vista previa de importación ya avisa de "posibles duplicados" por nombre además de por documento, ver tasks.md.
- **[Riesgo]** Un catálogo de ramos completamente abierto permite crear duplicados por variaciones de escritura (ej. "RC" vs "Responsabilidad Civil"). → **Mitigación**: el alta de ramo ya rechaza nombres duplicados exactos (sin distinguir mayúsculas/minúsculas); la normalización de sinónimos queda para una iteración futura si se vuelve un problema real.
- **[Riesgo]** Regenerar la migración inicial en vez de una migración incremental no sería seguro si ya hubiera datos reales cargados. → **Mitigación**: se verifica explícitamente antes de aplicar (ver tasks.md) que la base de datos de desarrollo no tiene datos reales, solo de prueba ya descartados.

## Migration Plan

1. Actualizar las entidades y `SegurosDbContext` con los cambios de esquema.
2. Eliminar la migración `InicialEsquema` existente (`dotnet ef migrations remove`).
3. Generar una nueva migración inicial desde cero con el esquema actualizado.
4. Sembrar el catálogo de `Ramo` con los nombres ya observados en los 3 archivos reales.
5. Aplicar la migración sobre una base de datos local limpia y verificar que el esquema se crea sin errores.

No hay plan de rollback de datos porque no hay datos reales que preservar en esta etapa.
