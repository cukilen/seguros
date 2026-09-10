## MODIFIED Requirements

### Requirement: Alta de asegurado
El sistema SHALL permitir registrar un nuevo asegurado con al menos nombre/razón social y un dato de contacto. El documento del asegurado (tipo: DNI, CUIT, LE u Otro, y número) es opcional al momento del alta, ya que no todas las fuentes de datos lo proveen.

#### Scenario: Alta exitosa
- **WHEN** el productor completa nombre, tipo y número de documento, y un dato de contacto, y confirma el alta
- **THEN** el sistema crea el asegurado con su documento y lo deja disponible para asociarle pólizas

#### Scenario: Alta exitosa sin documento
- **WHEN** el productor completa nombre y un dato de contacto, sin indicar documento, y confirma el alta
- **THEN** el sistema crea el asegurado sin documento y lo deja disponible para asociarle pólizas

#### Scenario: Documento duplicado
- **WHEN** el productor intenta dar de alta un asegurado con el mismo tipo y número de documento que uno ya registrado para ese mismo productor
- **THEN** el sistema rechaza el alta e indica que ya existe un asegurado con ese documento
