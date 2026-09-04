## Context

Ver `proposal.md` - Why para la motivación de negocio. A nivel técnico, el condicionante principal que definió este diseño es explícito del usuario: el sistema debe ser lo más fácil posible de instalar/copiar en otra PC, pensado para uso doméstico/oficina chica en una computadora común (Windows), sin depender de infraestructura de servidor ni de una conexión permanente a internet.

Es un proyecto greenfield: no hay código, base de datos ni sistemas previos que condicionen el diseño.

## Goals / Non-Goals

**Goals:**
- Minimizar dependencias externas: nada de instalar un motor de base de datos aparte, nada de configurar un servidor.
- Que copiar el programa a otra PC (o reinstalarlo) sea un paso simple, sin depender de un equipo de IT.
- Que funcione completamente offline (la carga de datos es manual o por importación de archivos, no depende de APIs externas).
- Soportar varios productores dentro de una misma instalación local, cada uno viendo solo su propia cartera.

**Non-Goals:**
- No es una arquitectura multi-tenant ni SaaS en la nube.
- No resuelve en esta etapa el acceso remoto ni la sincronización entre varias PCs/oficinas (ej. un productor viajando accediendo desde el celular). Si eso se vuelve necesario más adelante, implica una migración de arquitectura, no una extensión incremental.
- No incluye integración automática con los sistemas/portales de las compañías (ya excluido en `proposal.md`).

## Decisions

**1. Aplicación de escritorio, no aplicación web con servidor**
Se descarta una web app clásica (backend + navegador) porque agrega un proceso de servidor que hay que instalar, configurar y mantener corriendo - contrario al objetivo de "fácil de desplegar en otra PC para uso doméstico". Una app de escritorio se instala/copia y se ejecuta directamente.

**2. Stack: .NET (C#) con interfaz de escritorio (WPF), publicado self-contained**
- Alternativas consideradas:
  - *Electron*: requiere empaquetar Chromium + Node, instaladores pesados (~150 MB) y mayor consumo de recursos para una app de gestión simple.
  - *Python + Tkinter/PySide empaquetado con PyInstaller*: empaquetado menos confiable cuando hay librerías nativas (parsing de PDF/Excel), y la distribución "un solo ejecutable" es más frágil.
  - *.NET self-contained*: genera un ejecutable (o carpeta) que corre en Windows sin necesitar instalar el runtime de .NET en la PC destino. Buen ecosistema de librerías maduras para leer/escribir Excel y extraer texto de PDF, y para generar reportes.
- Se elige .NET + WPF por ser la opción con mejor balance entre facilidad de despliegue en una PC Windows común y madurez de librerías para los casos de uso de importación (XLS/PDF) y reportes (facturación).

**3. Base de datos: SQLite embebida (un solo archivo)**
- Alternativas consideradas: SQL Server / PostgreSQL - descartadas porque requieren instalar y administrar un motor de base de datos aparte, lo cual contradice el objetivo de despliegue simple.
- SQLite no requiere instalación: es un único archivo (ej. `seguros.db`) que viaja junto con la aplicación. Facilita además el backup (copiar el archivo).

**4. Multi-productor sin multi-tenancy de infraestructura**
Todos los productores conviven en la misma base SQLite local. Cada registro de asegurado/póliza queda asociado a un `productor_id`, y la capa de aplicación filtra la cartera visible según el productor logueado. No hay separación de bases de datos ni de instancias por productor - es aislamiento lógico, no físico.

**5. Importación XLS/PDF como "mejor esfuerzo" con revisión manual**
La lectura de archivos Excel (tabulares, estructura más predecible) se trata como el caso principal y más confiable. La extracción desde PDF depende del formato de cada compañía y puede ser inconsistente, por lo que el flujo de importación SIEMPRE muestra una vista previa de los datos detectados para que el productor revise/corrija antes de confirmar la carga - no se inserta nada directamente sin confirmación.

## Risks / Trade-offs

- **[Riesgo]** SQLite no está pensado para escritura concurrente intensa desde múltiples procesos o por red. → **Mitigación**: el uso previsto es una sola PC (o a lo sumo una red local chica); si más adelante se necesita acceso concurrente real desde varias máquinas, se evalúa migrar a un motor cliente-servidor.
- **[Riesgo]** Pérdida de datos si la PC falla y no existe backup. → **Mitigación**: incluir una función de backup del archivo de base de datos (copia manual o programada a una carpeta/USB) como parte del alcance de implementación.
- **[Riesgo]** La extracción de datos desde PDF puede fallar o traer datos mal interpretados por diferencias de formato entre compañías. → **Mitigación**: vista previa editable obligatoria antes de importar (ver Decisión 5); priorizar XLS como fuente principal.
- **[Riesgo]** Si el negocio crece y se necesita acceso remoto/multi-PC en tiempo real, esta arquitectura no lo soporta sin un rediseño. → **Mitigación**: se documenta como evolución futura fuera del alcance actual; no se cierra la puerta porque SQLite puede migrarse a otro motor si el modelo de datos está bien definido.

## Migration Plan

No aplica migración de datos (proyecto greenfield). Instalación inicial: copiar el ejecutable/carpeta de la aplicación a la PC destino; en el primer arranque la aplicación crea el archivo de base de datos SQLite vacío con el esquema inicial.

## Open Questions

- ¿Se necesita backup automático programado desde el inicio, o alcanza con una opción de backup manual en esta primera versión? No cambia las specs ni el enfoque, se puede definir durante la implementación.
- ¿La aplicación debe soportar Mac/Linux además de Windows? Se asume Windows únicamente por el contexto de uso (PC de oficina/hogar); confirmar si hace falta portabilidad a otro sistema operativo.
