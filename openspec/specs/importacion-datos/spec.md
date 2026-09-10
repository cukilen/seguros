# importacion-datos Specification

## Purpose
Permite importar listados de asegurados, pólizas y vencimientos a partir de archivos XLS y PDF entregados por las compañías, evitando la carga manual dato por dato.

## Requirements

### Requirement: Carga de archivo XLS con listado de pólizas o asegurados
El sistema SHALL permitir seleccionar un archivo XLS y leer su contenido como un listado tabular de asegurados y/o pólizas.

#### Scenario: Carga de archivo XLS válido
- **WHEN** el productor selecciona un archivo XLS con columnas de datos de pólizas
- **THEN** el sistema lee el archivo y muestra las filas detectadas para su revisión

### Requirement: Carga de archivo PDF con listado
El sistema SHALL permitir seleccionar un archivo PDF entregado por una compañía e intentar extraer de él un listado de asegurados, pólizas o vencimientos.

#### Scenario: Extracción exitosa desde PDF
- **WHEN** el productor selecciona un archivo PDF con un listado tabular reconocible
- **THEN** el sistema extrae los datos detectados y los muestra para su revisión, de la misma forma que una carga por XLS

#### Scenario: PDF sin datos reconocibles
- **WHEN** el productor selecciona un archivo PDF del cual no se puede extraer un listado estructurado
- **THEN** el sistema informa que no pudo interpretar el archivo y no importa datos

### Requirement: Mapeo de columnas a campos del sistema
El sistema SHALL permitir asociar (mapear) las columnas detectadas en el archivo importado con los campos correspondientes del sistema (por ejemplo, columna "Nº Póliza" con el campo número de póliza).

#### Scenario: Mapeo manual de columnas
- **WHEN** el sistema no reconoce automáticamente una columna del archivo
- **THEN** permite al productor indicar manualmente a qué campo del sistema corresponde esa columna antes de importar

### Requirement: Vista previa y confirmación antes de importar
El sistema SHALL mostrar una vista previa de los datos a importar, permitiendo corregirlos, antes de confirmar la carga definitiva.

#### Scenario: Corrección de un dato antes de confirmar
- **WHEN** el productor detecta un dato mal interpretado en la vista previa de importación
- **THEN** puede corregirlo manualmente antes de confirmar la importación, sin que el dato incorrecto se guarde en el sistema

#### Scenario: Detección de posible duplicado
- **WHEN** una fila a importar coincide con un asegurado o póliza ya existente en el sistema
- **THEN** el sistema señala el posible duplicado en la vista previa para que el productor decida si importar, omitir o actualizar ese registro

### Requirement: Reporte de resultado de importación
El sistema SHALL informar, al finalizar una importación, cuántos registros se importaron correctamente y cuántos quedaron con error.

#### Scenario: Importación con errores parciales
- **WHEN** una importación finaliza con algunas filas válidas y otras con error
- **THEN** el sistema importa las filas válidas e informa el detalle de las filas con error sin bloquear el resto

### Requirement: Determinación de la compañía al importar
El sistema SHALL determinar la compañía de los datos importados a partir de una columna del archivo cuando esa columna existe y está mapeada; cuando el archivo no trae una columna de compañía, el sistema SHALL permitir seleccionar una única compañía para todo el archivo antes de generar la vista previa.

#### Scenario: Archivo con columna de compañía
- **WHEN** el productor importa un archivo que trae una columna con el nombre de la compañía (por ejemplo un libro de movimientos que abarca varias compañías)
- **THEN** el sistema usa el valor de esa columna en cada fila para determinar la compañía de esa fila

#### Scenario: Archivo sin columna de compañía
- **WHEN** el productor importa un archivo que no trae ninguna columna de compañía (por ejemplo un export de cartera de una sola compañía)
- **THEN** el sistema le pide elegir una compañía antes de generar la vista previa, y la aplica a todas las filas del archivo

### Requirement: Carga de archivo CSV con listado
El sistema SHALL permitir seleccionar un archivo CSV (u otro formato delimitado por punto y coma o coma) y leerlo como un listado tabular, detectando el delimitador automáticamente.

#### Scenario: Carga de archivo CSV válido
- **WHEN** el productor selecciona un archivo CSV delimitado por punto y coma con columnas de datos de pólizas
- **THEN** el sistema detecta el delimitador, lee el archivo y muestra las filas detectadas para su revisión, igual que con un archivo XLS

### Requirement: Selección de la fila de encabezado
El sistema SHALL permitir indicar en qué fila está el encabezado real de un archivo XLS o CSV, contando las filas como se ven en el archivo (incluyendo filas en blanco), para admitir archivos que traen filas de título antes del encabezado.

#### Scenario: Archivo con filas de título antes del encabezado
- **WHEN** el productor importa un archivo cuyo encabezado real no está en la primera fila (por ejemplo, tiene un título y un código de referencia arriba) e indica el número de esa fila
- **THEN** el sistema usa esa fila como encabezado y lee los datos a partir de la fila siguiente

### Requirement: Estado de la póliza según el archivo
El sistema SHALL permitir mapear una columna del archivo al estado de la póliza (Vigente, Anulada, Renovada o No vigente); si no se mapea ninguna columna a este campo, el sistema SHALL asumir que la póliza está vigente.

#### Scenario: Archivo con columna de estado
- **WHEN** el productor mapea una columna del archivo al campo Estado y una fila indica, por ejemplo, "Anulada"
- **THEN** la póliza se crea o actualiza con ese estado en lugar de asumirse vigente

#### Scenario: Archivo sin columna de estado
- **WHEN** el archivo no tiene ninguna columna mapeada al campo Estado
- **THEN** el sistema crea las pólizas como vigentes, igual que antes de que existiera este campo

### Requirement: Deduplicación de historial por fila más reciente
El sistema SHALL permitir, para archivos que traen varias filas para la misma póliza (historial de movimientos), quedarse con una única fila por póliza según la fecha más reciente de una columna indicada, antes de generar la vista previa de importación.

#### Scenario: Archivo con varias filas por póliza
- **WHEN** el productor indica que el archivo trae varias filas por póliza y cuál es la columna de fecha a usar para determinar la más reciente
- **THEN** el sistema conserva solo la fila más reciente de cada póliza y descarta el resto antes de mostrar la vista previa

### Requirement: Tolerancia de formato en montos y prima
El sistema SHALL reconocer montos con prefijos de moneda usuales (por ejemplo "$", "U$S", "USD", "AR$") sin importar el resto; y SHALL permitir importar una póliza sin monto de prima cuando el archivo no trae esa columna, quedando la prima en cero, sin rechazar la fila. Una columna de prima mapeada con un valor no interpretable SHALL seguir rechazando esa fila.

#### Scenario: Monto con prefijo de moneda
- **WHEN** una fila trae un monto como "U$S250.49"
- **THEN** el sistema lo interpreta correctamente en lugar de marcarlo como valor inválido

#### Scenario: Archivo sin columna de prima
- **WHEN** el productor importa un archivo que no tiene ninguna columna mapeada al campo Prima
- **THEN** el sistema importa la póliza con prima cero en vez de rechazar la fila por falta de ese dato
