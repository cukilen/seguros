## Purpose

Permite importar listados de asegurados, pólizas y vencimientos a partir de archivos XLS y PDF entregados por las compañías, evitando la carga manual dato por dato.

## ADDED Requirements

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
