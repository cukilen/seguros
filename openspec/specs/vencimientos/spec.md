# vencimientos Specification

## Purpose
Hace seguimiento de las fechas de vencimiento de las pólizas y genera alertas dentro del sistema para que el productor pueda anticipar la gestión de renovaciones.

## Requirements

### Requirement: Cálculo del vencimiento de una póliza
El sistema SHALL calcular el vencimiento de una póliza a partir de su fecha de fin de vigencia.

#### Scenario: Póliza con fecha de fin de vigencia cargada
- **WHEN** se registra o edita una póliza con una fecha de fin de vigencia
- **THEN** el sistema actualiza el vencimiento asociado a esa póliza para reflejar esa fecha

### Requirement: Alerta interna de póliza próxima a vencer
El sistema SHALL mostrar una alerta dentro del sistema para las pólizas cuyo vencimiento esté dentro de una ventana configurable de días (por ejemplo 30, 15 o 7 días).

#### Scenario: Póliza dentro de la ventana de alerta
- **WHEN** faltan 15 días o menos para el vencimiento de una póliza vigente
- **THEN** el sistema la incluye en el listado de alertas de vencimientos próximos

#### Scenario: Sin notificaciones externas
- **WHEN** una póliza entra en la ventana de alerta
- **THEN** el sistema muestra la alerta dentro de la aplicación, sin enviar notificaciones por email ni WhatsApp

### Requirement: Listado de pólizas vencidas y por vencer
El sistema SHALL permitir listar las pólizas vencidas y las próximas a vencer, filtrables por productor, compañía y ramo.

#### Scenario: Filtrado de vencimientos por compañía
- **WHEN** el productor filtra el listado de vencimientos por una compañía específica
- **THEN** el sistema muestra únicamente las pólizas de esa compañía próximas a vencer o vencidas

### Requirement: Marcar un vencimiento como gestionado
El sistema SHALL permitir marcar el vencimiento de una póliza como gestionado (renovada o dada de baja) para que deje de aparecer como pendiente en las alertas.

#### Scenario: Vencimiento resuelto por renovación
- **WHEN** el productor renueva una póliza que estaba en la lista de vencimientos próximos
- **THEN** el sistema deja de mostrarla como pendiente de gestión y refleja el nuevo vencimiento según la renovación
