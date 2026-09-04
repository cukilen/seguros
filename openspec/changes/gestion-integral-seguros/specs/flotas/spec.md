## Purpose

Permite administrar pólizas de flota del ramo autos, que agrupan múltiples vehículos/unidades bajo un mismo contrato, con altas y bajas individuales de unidades.

## ADDED Requirements

### Requirement: Alta de póliza de flota
El sistema SHALL permitir marcar una póliza del ramo autos como póliza de flota, habilitando la carga de múltiples unidades dentro de ella.

#### Scenario: Alta de póliza de flota
- **WHEN** el productor crea una póliza del ramo autos y la marca como flota
- **THEN** el sistema habilita el alta de unidades/vehículos dentro de esa póliza

### Requirement: Alta de unidad dentro de una flota
El sistema SHALL permitir agregar una unidad (vehículo) a una póliza de flota, con al menos patente, marca, modelo y uso.

#### Scenario: Alta de unidad exitosa
- **WHEN** el productor agrega una unidad con patente, marca, modelo y uso a una póliza de flota
- **THEN** el sistema incorpora la unidad a la flota, quedando activa dentro de esa póliza

#### Scenario: Patente duplicada dentro de la misma flota
- **WHEN** el productor intenta agregar una unidad con una patente ya activa en esa misma flota
- **THEN** el sistema rechaza el alta e indica que la unidad ya está registrada en esa flota

### Requirement: Baja de unidad dentro de una flota
El sistema SHALL permitir dar de baja una unidad de una flota sin afectar al resto de las unidades ni dar de baja la póliza completa.

#### Scenario: Baja de una unidad
- **WHEN** el productor da de baja una unidad de una flota indicando la fecha de baja
- **THEN** el sistema marca esa unidad como inactiva y mantiene el resto de las unidades de la flota sin cambios

### Requirement: Listado de unidades de una flota
El sistema SHALL permitir consultar el listado de unidades activas e inactivas de una póliza de flota.

#### Scenario: Consulta del listado de unidades
- **WHEN** el productor abre una póliza de flota
- **THEN** el sistema muestra todas las unidades asociadas, indicando cuáles están activas y cuáles dadas de baja
