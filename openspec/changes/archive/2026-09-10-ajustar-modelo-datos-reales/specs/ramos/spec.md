## Purpose

Mantiene un catálogo abierto de ramos/secciones de seguro (por ejemplo Autos, Vida, Incendio, Combinado Familiar, Caución, Transporte) para representar la variedad real de nomenclaturas que usa cada compañía, sin forzar un listado cerrado predefinido.

## ADDED Requirements

### Requirement: Alta de ramo en el catálogo
El sistema SHALL permitir registrar un nuevo ramo en el catálogo con un nombre.

#### Scenario: Alta de un ramo nuevo
- **WHEN** se registra un ramo con un nombre que no existe todavía en el catálogo
- **THEN** el sistema lo agrega y lo deja disponible para asociarlo a compañías y pólizas

#### Scenario: Nombre de ramo duplicado
- **WHEN** se intenta registrar un ramo con un nombre ya existente en el catálogo (sin distinguir mayúsculas/minúsculas)
- **THEN** el sistema rechaza el alta e indica que ese ramo ya existe

### Requirement: Listado de ramos del catálogo
El sistema SHALL permitir consultar el listado completo de ramos disponibles en el catálogo.

#### Scenario: Consulta del catálogo
- **WHEN** el productor necesita elegir un ramo (por ejemplo al dar de alta una póliza o al mapear una importación)
- **THEN** el sistema le ofrece el catálogo completo de ramos existentes

### Requirement: Los ramos se referencian, no se codifican de forma fija
El sistema SHALL permitir asociar compañías y pólizas a cualquier ramo del catálogo, sin limitarse a un conjunto fijo predefinido de nombres.

#### Scenario: Ramo con nomenclatura específica de una compañía
- **WHEN** una compañía usa un nombre de ramo que no es uno de los más comunes (por ejemplo "Vida Colectivo Abierto" o "Transporte de Cascos")
- **THEN** el sistema permite darlo de alta en el catálogo y usarlo igual que cualquier otro ramo
