## ADDED Requirements

### Requirement: Determinación de la compañía al importar
El sistema SHALL determinar la compañía de los datos importados a partir de una columna del archivo cuando esa columna existe y está mapeada; cuando el archivo no trae una columna de compañía, el sistema SHALL permitir seleccionar una única compañía para todo el archivo antes de generar la vista previa.

#### Scenario: Archivo con columna de compañía
- **WHEN** el productor importa un archivo que trae una columna con el nombre de la compañía (por ejemplo un libro de movimientos que abarca varias compañías)
- **THEN** el sistema usa el valor de esa columna en cada fila para determinar la compañía de esa fila

#### Scenario: Archivo sin columna de compañía
- **WHEN** el productor importa un archivo que no trae ninguna columna de compañía (por ejemplo un export de cartera de una sola compañía)
- **THEN** el sistema le pide elegir una compañía antes de generar la vista previa, y la aplica a todas las filas del archivo
