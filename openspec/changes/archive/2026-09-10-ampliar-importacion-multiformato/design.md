## Context

Ver proposal.md - Why. Estos cambios se hicieron mientras se cargaban a la base real los 3 archivos analizados anteriormente (Mapfre, La Holando, LIDERAR), así que ya están probados contra datos reales, no solo contra tests.

## Goals / Non-Goals

**Goals:**
- Que el importador funcione con los formatos de archivo reales que el productor recibe (CSV, filas de título antes del encabezado, estado de póliza, montos con prefijo de moneda).
- No romper el comportamiento existente cuando un archivo no trae estos datos (todo sigue funcionando como antes por defecto).

**Non-Goals:**
- No se resuelve la generalización completa de "importar cualquier archivo sin configuración": el productor sigue teniendo que mapear columnas y, en archivos con historial, indicar la columna de fecha para deduplicar.
- No se modela un historial de endosos a partir de las filas descartadas en la deduplicación (se pierden, ver Riesgos).

## Decisions

**1. La fila de encabezado se cuenta como en Excel, no como "fila usada Nº N"**
La primera implementación indexaba sobre `RowsUsed()` (que salta filas en blanco), así que "fila de encabezado 7" en un archivo con filas en blanco entre el título y el encabezado apuntaba a una fila de datos, no al encabezado real. Se corrigió para direccionar la fila por su número real de Excel (`hoja.Row(n)`), que es como un humano cuenta filas al mirar el archivo.

**2. Prima ausente → 0, no error; prima presente pero inválida → sigue rechazando la fila**
Se distingue "la columna no está mapeada" (se asume 0) de "la columna está mapeada pero el valor no se pudo interpretar" (se rechaza esa fila). Alternativa descartada: exigir prima siempre, que hubiera bloqueado por completo el archivo de LIDERAR (un libro de movimientos que no incluye prima).

**3. Deduplicación por fecha más reciente, no por número de fila más alto**
Se usa la fecha (parseada con `ParseoImportacion.TryParseFecha`) de una columna indicada por el productor, no simplemente "la última fila del grupo", porque no todos los archivos vienen ordenados cronológicamente de la misma forma.

## Cómo se usó para la carga inicial real (nota operativa, no parte del sistema)

Para cargar los 3 archivos se usaron mapeos y criterios específicos de cada fuente que no quedaron guardados en el sistema (se hizo con un script puntual que llamó a los mismos servicios que usa la pantalla de Importación):
- **Mapfre**: no tenía fecha de inicio de vigencia, solo una fecha de vencimiento/renovación. Se estimó el inicio restando 1 mes (facturación mensual) o 1 año (el resto) a esa fecha. Es una aproximación; las fechas de las pólizas de Mapfre importadas conviene revisarlas.
- **LIDERAR**: no tiene columna de estado, sino "Tipo Movimiento". Se derivó Anulada cuando ese texto contenía "ANULACION", y Vigente en cualquier otro caso.

## Risks / Trade-offs

- **[Riesgo]** Al deduplicar un historial de movimientos, las filas descartadas (endosos/movimientos intermedios) no quedan registradas como `Endoso` en el sistema, se pierden. → **Mitigación**: aceptado por ahora; si más adelante se necesita el historial completo, se puede extender la importación para crear un `Endoso` por cada fila descartada en vez de solo ignorarla.
- **[Riesgo]** La estimación de fecha de inicio de vigencia de Mapfre (ver nota operativa) no es un dato real. → **Mitigación**: queda documentado acá; el productor puede corregir esas fechas manualmente desde la pantalla de Pólizas.
