## ADDED Requirements

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
