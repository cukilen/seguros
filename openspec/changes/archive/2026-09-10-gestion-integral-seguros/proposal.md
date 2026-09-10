## Why

Los productores de seguros que operan con varias compañías no cuentan con una vista unificada de sus asegurados: hoy la información vive dispersa entre los sistemas y portales de cada compañía. Un mismo asegurado puede tener pólizas de distintos ramos (vida, incendio, RC, autos/flotas, combinado familiar, accidentes personales) contratadas en distintas aseguradoras, lo que obliga al productor a consultar múltiples fuentes para saber qué tiene cada cliente, cuándo vence cada póliza y qué endosos se le aplicaron. Esto genera pérdida de renovaciones, atención lenta al asegurado y riesgo de perder comisiones y clientes frente a la competencia. Se necesita un sistema propio que centralice toda la cartera del productor, independientemente de la compañía o el ramo.

## What Changes

- Se construye un sistema nuevo (greenfield) de gestión integral de cartera para productores de seguros.
- Alta y administración de compañías aseguradoras con las que opera el productor.
- Alta y administración de asegurados, con vista unificada de todas sus pólizas sin importar compañía o ramo.
- Gestión del ciclo de vida de pólizas multi-compañía y multi-ramo (vida, incendio, RC, autos, combinado familiar, accidentes personales).
- Gestión de pólizas de flota, permitiendo administrar múltiples unidades/vehículos bajo una misma póliza.
- Registro y seguimiento de endosos (modificaciones posteriores a la emisión) sobre pólizas existentes.
- Seguimiento y alertas de vencimientos de pólizas para anticipar renovaciones.
- Importación de listados de asegurados, pólizas y renovaciones desde archivos XLS y PDF que entregan las compañías, evitando la carga manual.
- Soporte para múltiples productores/usuarios en el sistema, cada uno con su propia cartera de asegurados.
- Facturación y liquidación de comisiones por póliza/compañía para cada productor.
- No hay cambios **BREAKING**: es un sistema nuevo, sin base instalada previa.

## Capabilities

### New Capabilities
- `companias`: Alta, edición y consulta de compañías aseguradoras con las que trabaja el productor (datos de contacto, ramos que operan, condiciones comerciales).
- `asegurados`: Alta, edición y consulta de asegurados (clientes), con vista consolidada de todas sus pólizas en todas las compañías y ramos.
- `polizas`: Gestión del ciclo de vida de una póliza (emisión, vigencia, renovación, baja), asociada a un asegurado, una compañía y un ramo (vida, incendio, RC, autos, combinado familiar, accidentes personales).
- `flotas`: Gestión de pólizas de flota de vehículos: alta/baja de unidades dentro de una misma póliza y datos particulares de cada vehículo asegurado.
- `endosos`: Registro y seguimiento de endosos sobre pólizas existentes (qué cambió, cuándo y por qué).
- `vencimientos`: Seguimiento de fechas de vencimiento de pólizas y alertas dentro del sistema (no notificaciones externas por email/WhatsApp en esta etapa) para gestionar la renovación a tiempo.
- `importacion-datos`: Importación de listados de asegurados, pólizas y renovaciones a partir de archivos XLS y PDF entregados por las compañías, con mapeo de columnas/campos y carga hacia `asegurados`, `polizas` y `vencimientos`.
- `productores`: Alta y administración de productores/usuarios del sistema; cada productor gestiona su propia cartera de asegurados y pólizas.
- `facturacion-comisiones`: Registro y liquidación de comisiones por póliza y por compañía para cada productor.

### Modified Capabilities
<!-- No existen specs previas: proyecto greenfield, sin capacidades existentes que modificar. -->

## Impact

- Sistema nuevo: no afecta código, APIs ni sistemas existentes (no hay base previa en este repositorio).
- El stack técnico, modelo de datos y arquitectura se definirán en `design.md`.
- Fuera de alcance inicial (a evaluar más adelante): integraciones automáticas con los sistemas/portales de cada compañía aseguradora, notificaciones externas de vencimientos (email/WhatsApp).
