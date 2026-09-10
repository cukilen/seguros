# polizas Specification

## Purpose
Gestiona el ciclo de vida de las pólizas emitidas a los asegurados, asociando cada póliza a un asegurado, una compañía y un ramo (vida, incendio, RC, autos, combinado familiar, accidentes personales).

## Requirements

### Requirement: Alta de póliza
El sistema SHALL permitir registrar una póliza asociada a un asegurado, una compañía y un ramo, con número de póliza, fecha de inicio y fin de vigencia, y prima.

#### Scenario: Alta exitosa
- **WHEN** el productor completa asegurado, compañía, ramo, número de póliza, vigencia y prima, y confirma el alta
- **THEN** el sistema crea la póliza en estado vigente y la asocia al asegurado y a la compañía indicados

#### Scenario: Número de póliza duplicado en la misma compañía
- **WHEN** el productor intenta dar de alta una póliza con un número que ya existe para esa misma compañía
- **THEN** el sistema rechaza el alta e indica que el número de póliza ya está en uso para esa compañía

### Requirement: Edición de póliza vigente
El sistema SHALL permitir editar los datos de una póliza vigente (por ejemplo prima o fechas de vigencia), preservando la trazabilidad de que hubo un cambio.

#### Scenario: Edición de la prima de una póliza
- **WHEN** el productor modifica la prima de una póliza vigente
- **THEN** el sistema guarda el nuevo valor y mantiene registro de que la póliza fue modificada

### Requirement: Renovación de póliza
El sistema SHALL permitir renovar una póliza próxima a vencer o vencida, generando un nuevo período de vigencia vinculado a la póliza original.

#### Scenario: Renovación exitosa
- **WHEN** el productor renueva una póliza indicando la nueva fecha de fin de vigencia
- **THEN** el sistema actualiza la vigencia de la póliza y queda registrado que fue renovada

### Requirement: Baja o anulación de póliza
El sistema SHALL permitir dar de baja o anular una póliza, dejando de considerarla vigente.

#### Scenario: Anulación de póliza
- **WHEN** el productor anula una póliza indicando el motivo
- **THEN** el sistema marca la póliza como anulada y deja de incluirla entre las pólizas vigentes del asegurado

### Requirement: Consulta de pólizas por asegurado, compañía o ramo
El sistema SHALL permitir listar y filtrar pólizas por asegurado, por compañía y por ramo.

#### Scenario: Filtrado por ramo
- **WHEN** el productor filtra el listado de pólizas por el ramo "autos"
- **THEN** el sistema muestra únicamente las pólizas de ese ramo, sin importar la compañía o el asegurado
