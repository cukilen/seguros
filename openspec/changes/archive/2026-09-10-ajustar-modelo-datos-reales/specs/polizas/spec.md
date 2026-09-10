## MODIFIED Requirements

### Requirement: Alta de póliza
El sistema SHALL permitir registrar una póliza asociada a un asegurado, una compañía y un ramo del catálogo, con número de póliza, fecha de inicio y fin de vigencia, y prima. El producto (nombre comercial del plan), el premio (monto total, distinto de la prima neta) y la cobertura (nivel de cobertura, relevante en el ramo autos) son datos opcionales adicionales.

#### Scenario: Alta exitosa
- **WHEN** el productor completa asegurado, compañía, ramo, número de póliza, vigencia y prima, y confirma el alta
- **THEN** el sistema crea la póliza en estado vigente y la asocia al asegurado y a la compañía indicados

#### Scenario: Alta con producto, premio y cobertura
- **WHEN** el productor completa además el producto, el premio y la cobertura de una póliza del ramo autos
- **THEN** el sistema guarda esos datos junto con la póliza

#### Scenario: Número de póliza duplicado en la misma compañía
- **WHEN** el productor intenta dar de alta una póliza con un número que ya existe para esa misma compañía
- **THEN** el sistema rechaza el alta e indica que el número de póliza ya está en uso para esa compañía

## ADDED Requirements

### Requirement: Marcado de póliza como no vigente
El sistema SHALL permitir marcar una póliza vigente como no vigente cuando venció sin ser renovada ni anulada explícitamente, distinguiendo ese caso de una anulación.

#### Scenario: Póliza vencida sin gestión
- **WHEN** una póliza vigente pasó su fecha de vencimiento sin haber sido renovada ni anulada
- **THEN** el productor puede marcarla como no vigente, y el sistema deja de contarla como vigente sin registrarla como anulada

### Requirement: Descripción simple de riesgo para pólizas sin flota
El sistema SHALL permitir registrar una descripción de riesgo en texto libre (por ejemplo, un vehículo) directamente en una póliza que no está marcada como flota.

#### Scenario: Póliza individual con un solo riesgo
- **WHEN** el productor da de alta o edita una póliza que no es de flota y completa una descripción de riesgo
- **THEN** el sistema guarda esa descripción junto con la póliza, sin requerir la estructura de unidades de flota
