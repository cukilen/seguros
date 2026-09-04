## Purpose

Registra la facturación asociada a las pólizas y liquida las comisiones que corresponden a cada productor por compañía y ramo.

## ADDED Requirements

### Requirement: Registro de porcentaje de comisión por compañía y ramo
El sistema SHALL permitir registrar el porcentaje de comisión pactado para una combinación de compañía y ramo.

#### Scenario: Alta de comisión pactada
- **WHEN** se registra un porcentaje de comisión para el ramo "autos" de una compañía determinada
- **THEN** el sistema usa ese porcentaje para calcular la comisión de las pólizas de ese ramo y esa compañía

### Requirement: Cálculo de comisión por póliza
El sistema SHALL calcular la comisión correspondiente a una póliza emitida o renovada, en base a la prima de la póliza y el porcentaje de comisión pactado para su compañía y ramo.

#### Scenario: Cálculo automático al emitir una póliza
- **WHEN** se emite una póliza cuya compañía y ramo tienen un porcentaje de comisión configurado
- **THEN** el sistema calcula el monto de comisión correspondiente a esa póliza

#### Scenario: Póliza sin porcentaje de comisión configurado
- **WHEN** se emite una póliza de una compañía/ramo sin porcentaje de comisión configurado
- **THEN** el sistema la deja pendiente de comisión e indica que falta configurar el porcentaje

### Requirement: Liquidación de comisiones por productor y período
El sistema SHALL permitir generar una liquidación de comisiones para un productor correspondiente a un período determinado, agrupando las pólizas emitidas o renovadas en ese lapso.

#### Scenario: Generación de liquidación de un período
- **WHEN** el productor genera la liquidación de comisiones de un mes determinado
- **THEN** el sistema totaliza las comisiones de todas las pólizas de ese productor emitidas o renovadas en ese período

### Requirement: Consulta de historial de liquidaciones
El sistema SHALL permitir consultar las liquidaciones de comisiones generadas anteriormente para un productor.

#### Scenario: Consulta de liquidaciones previas
- **WHEN** el productor abre el historial de liquidaciones
- **THEN** el sistema muestra las liquidaciones generadas anteriormente con su período y monto total
