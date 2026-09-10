## Purpose

Permite registrar y administrar a los asegurados (clientes) del productor, ofreciendo una vista consolidada de todas sus pólizas sin importar la compañía o el ramo en que estén contratadas.

## ADDED Requirements

### Requirement: Alta de asegurado
El sistema SHALL permitir registrar un nuevo asegurado con al menos nombre/razón social, documento (DNI/CUIT) y un dato de contacto.

#### Scenario: Alta exitosa
- **WHEN** el productor completa nombre, documento y un dato de contacto, y confirma el alta
- **THEN** el sistema crea el asegurado y lo deja disponible para asociarle pólizas

#### Scenario: Documento duplicado
- **WHEN** el productor intenta dar de alta un asegurado con un documento ya registrado para ese mismo productor
- **THEN** el sistema rechaza el alta e indica que ya existe un asegurado con ese documento

### Requirement: Edición de datos de asegurado
El sistema SHALL permitir editar los datos de contacto y personales de un asegurado existente.

#### Scenario: Actualización de datos de contacto
- **WHEN** el productor modifica el teléfono, email o domicilio de un asegurado
- **THEN** el sistema guarda los cambios y los refleja en las consultas posteriores

### Requirement: Vista consolidada de pólizas de un asegurado
El sistema SHALL mostrar, para un asegurado dado, todas sus pólizas vigentes e históricas sin importar la compañía o el ramo en que fueron contratadas.

#### Scenario: Asegurado con pólizas en varias compañías y ramos
- **WHEN** el productor abre la ficha de un asegurado que tiene una póliza de auto en una compañía y una póliza de vida en otra
- **THEN** el sistema muestra ambas pólizas en la misma vista, indicando compañía, ramo y estado de cada una

### Requirement: Búsqueda de asegurados
El sistema SHALL permitir buscar asegurados por nombre o número de documento.

#### Scenario: Búsqueda por documento
- **WHEN** el productor busca por número de documento
- **THEN** el sistema muestra el asegurado correspondiente si existe

### Requirement: Baja de asegurado con pólizas vigentes
El sistema SHALL impedir eliminar un asegurado que tiene pólizas vigentes, permitiendo únicamente inactivarlo.

#### Scenario: Intento de eliminar asegurado con pólizas vigentes
- **WHEN** el productor intenta eliminar un asegurado con al menos una póliza vigente
- **THEN** el sistema rechaza la eliminación y ofrece inactivar el asegurado en su lugar
