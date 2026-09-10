## Purpose

Administra los productores/usuarios que acceden al sistema, permitiendo que varios productores usen la misma instalación local, cada uno con su propia cartera de asegurados y pólizas.

## ADDED Requirements

### Requirement: Alta de productor
El sistema SHALL permitir registrar un nuevo productor con nombre y credenciales de acceso propias.

#### Scenario: Alta de productor exitosa
- **WHEN** se registra un nuevo productor con nombre, usuario y contraseña
- **THEN** el sistema crea el productor y le permite iniciar sesión con esas credenciales

### Requirement: Inicio de sesión de productor
El sistema SHALL requerir que cada productor inicie sesión con sus credenciales antes de acceder a los datos.

#### Scenario: Inicio de sesión exitoso
- **WHEN** un productor ingresa un usuario y contraseña válidos
- **THEN** el sistema le da acceso a su propia cartera de asegurados y pólizas

#### Scenario: Credenciales inválidas
- **WHEN** un productor ingresa una contraseña incorrecta
- **THEN** el sistema rechaza el inicio de sesión y no otorga acceso a ningún dato

### Requirement: Aislamiento de cartera entre productores
El sistema SHALL mostrar a cada productor únicamente los asegurados y pólizas que le pertenecen a él, sin acceso a la cartera de otros productores.

#### Scenario: Productor sin acceso a datos de otro productor
- **WHEN** un productor consulta el listado de asegurados
- **THEN** el sistema muestra solo los asegurados que fueron dados de alta bajo ese productor, sin incluir los de otros productores

### Requirement: Edición y baja de productor
El sistema SHALL permitir editar los datos de un productor existente y desactivar su acceso sin eliminar su cartera histórica.

#### Scenario: Desactivación de un productor
- **WHEN** se desactiva el acceso de un productor
- **THEN** el sistema le impide iniciar sesión mientras conserva sus asegurados y pólizas históricas
