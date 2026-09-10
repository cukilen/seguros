## Purpose

Permite registrar y administrar las compañías aseguradoras con las que opera el productor, incluyendo los ramos que cada una opera y sus datos de contacto.

## ADDED Requirements

### Requirement: Alta de compañía aseguradora
El sistema SHALL permitir registrar una nueva compañía aseguradora con al menos nombre y los ramos que opera.

#### Scenario: Alta exitosa
- **WHEN** el productor completa nombre y al menos un ramo operado, y confirma el alta
- **THEN** el sistema crea la compañía y la deja disponible para asociarla a pólizas

#### Scenario: Falta el nombre de la compañía
- **WHEN** el productor intenta dar de alta una compañía sin nombre
- **THEN** el sistema rechaza el alta e indica que el nombre es obligatorio

### Requirement: Edición de datos de compañía
El sistema SHALL permitir editar los datos de contacto y los ramos operados de una compañía existente.

#### Scenario: Actualización de datos de contacto
- **WHEN** el productor modifica el teléfono o email de una compañía existente y guarda los cambios
- **THEN** el sistema actualiza los datos de la compañía

### Requirement: Consulta y listado de compañías
El sistema SHALL permitir listar y consultar el detalle de las compañías registradas, incluyendo los ramos que opera cada una.

#### Scenario: Listado de compañías
- **WHEN** el productor abre el listado de compañías
- **THEN** el sistema muestra todas las compañías registradas junto con los ramos que opera cada una

### Requirement: Baja de compañía con pólizas asociadas
El sistema SHALL impedir eliminar una compañía que tiene pólizas asociadas, permitiendo únicamente inactivarla.

#### Scenario: Intento de eliminar compañía con pólizas vigentes
- **WHEN** el productor intenta eliminar una compañía que tiene al menos una póliza asociada
- **THEN** el sistema rechaza la eliminación y ofrece inactivar la compañía en su lugar

#### Scenario: Inactivación de compañía sin pólizas
- **WHEN** el productor inactiva una compañía sin pólizas asociadas
- **THEN** el sistema marca la compañía como inactiva y deja de ofrecerla para nuevas altas de pólizas
