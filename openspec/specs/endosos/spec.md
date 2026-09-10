# endosos Specification

## Purpose
Registra y da seguimiento a los endosos (modificaciones posteriores a la emisión) aplicados a una póliza vigente a lo largo de su vida.

## Requirements

### Requirement: Alta de endoso sobre una póliza
El sistema SHALL permitir registrar un endoso sobre una póliza existente, indicando tipo de cambio, fecha y motivo/detalle.

#### Scenario: Alta de endoso exitosa
- **WHEN** el productor registra un endoso sobre una póliza indicando tipo de cambio, fecha y detalle
- **THEN** el sistema guarda el endoso asociado a esa póliza

#### Scenario: Endoso sobre póliza anulada
- **WHEN** el productor intenta registrar un endoso sobre una póliza que ya está anulada
- **THEN** el sistema rechaza el alta e indica que la póliza no está vigente

### Requirement: Historial de endosos de una póliza
El sistema SHALL permitir consultar el historial completo de endosos de una póliza, ordenado cronológicamente.

#### Scenario: Consulta del historial
- **WHEN** el productor abre una póliza con varios endosos registrados
- **THEN** el sistema muestra todos los endosos ordenados desde el más antiguo al más reciente

### Requirement: Anulación de un endoso mal cargado
El sistema SHALL permitir anular un endoso cargado por error, dejando constancia de la anulación sin borrar el registro original.

#### Scenario: Anulación de endoso
- **WHEN** el productor anula un endoso indicando el motivo de la anulación
- **THEN** el sistema marca el endoso como anulado y lo mantiene visible en el historial con su estado
