## 1. Configuración inicial del proyecto

- [x] 1.1 Crear la solución .NET (WPF, self-contained) con la estructura de proyectos (UI, dominio/lógica de negocio, acceso a datos) y verificar que compila sin errores
- [x] 1.2 Agregar SQLite y el ORM/acceso a datos elegido (ej. EF Core con proveedor SQLite) y verificar que la aplicación puede crear un archivo de base de datos vacío al iniciar
- [x] 1.3 Agregar dependencias para lectura de Excel (ej. ClosedXML) y extracción de texto de PDF (ej. PdfPig) y verificar que ambas librerías se referencian correctamente en el build
- [x] 1.4 Configurar publicación self-contained para Windows y verificar que se genera un ejecutable/carpeta que corre en una PC sin el runtime de .NET instalado

## 2. Modelo de datos base

- [x] 2.1 Definir el esquema de base de datos para productor, compañía, ramo, asegurado, póliza, unidad de flota, endoso, comisión y liquidación, y verificar que las migraciones/creación de esquema se aplican sin errores sobre una base SQLite vacía
- [x] 2.2 Verificar con datos de prueba que las relaciones clave (póliza→asegurado, póliza→compañía, póliza→productor, unidad→póliza de flota, endoso→póliza) se pueden crear y consultar correctamente

## 3. Productores y acceso (spec: productores)

- [x] 3.1 Implementar alta de productor con usuario y contraseña, y verificar que un productor recién creado puede iniciar sesión
- [x] 3.2 Implementar inicio de sesión y verificar que credenciales inválidas son rechazadas sin otorgar acceso
- [x] 3.3 Implementar el filtrado de datos por productor logueado (asegurados, pólizas) y verificar con dos productores de prueba que ninguno ve la cartera del otro
- [x] 3.4 Implementar edición y desactivación de productor, y verificar que un productor desactivado no puede iniciar sesión pero su cartera histórica sigue accesible para consulta administrativa

## 4. Compañías aseguradoras (spec: companias)

- [x] 4.1 Implementar alta de compañía con nombre y ramos operados, y verificar que el alta sin nombre es rechazada
- [x] 4.2 Implementar edición de datos de contacto de una compañía y verificar que los cambios persisten
- [x] 4.3 Implementar listado/consulta de compañías con sus ramos operados y verificar que se muestran correctamente
- [x] 4.4 Implementar inactivación de compañía y bloqueo de eliminación cuando tiene pólizas asociadas, verificando ambos casos (con y sin pólizas)

## 5. Asegurados (spec: asegurados)

- [x] 5.1 Implementar alta de asegurado con validación de documento único por productor, y verificar el caso de documento duplicado
- [x] 5.2 Implementar edición de datos de asegurado y verificar que los cambios persisten
- [x] 5.3 Implementar la vista consolidada de pólizas de un asegurado (todas las compañías y ramos) y verificar con un asegurado de prueba que tiene pólizas en más de una compañía
- [x] 5.4 Implementar búsqueda de asegurados por nombre y por documento, y verificar ambos criterios de búsqueda
- [x] 5.5 Implementar inactivación de asegurado y bloqueo de eliminación cuando tiene pólizas vigentes

## 6. Pólizas (spec: polizas)

- [x] 6.1 Implementar alta de póliza (asegurado, compañía, ramo, número, vigencia, prima) y verificar que un número de póliza duplicado dentro de la misma compañía es rechazado
- [x] 6.2 Implementar edición de póliza vigente (prima, vigencia) y verificar que los cambios quedan registrados
- [x] 6.3 Implementar renovación de póliza y verificar que genera una nueva vigencia vinculada a la póliza original
- [x] 6.4 Implementar baja/anulación de póliza con motivo y verificar que deja de listarse entre las pólizas vigentes del asegurado
- [x] 6.5 Implementar consulta y filtrado de pólizas por asegurado, compañía y ramo, y verificar el filtrado por ramo

## 7. Flotas (spec: flotas)

- [x] 7.1 Implementar marcado de una póliza de ramo autos como póliza de flota y verificar que habilita el alta de unidades
- [x] 7.2 Implementar alta de unidad (patente, marca, modelo, uso) dentro de una flota y verificar que una patente duplicada en la misma flota es rechazada
- [x] 7.3 Implementar baja de unidad dentro de una flota y verificar que el resto de las unidades y la póliza no se ven afectadas
- [x] 7.4 Implementar listado de unidades activas e inactivas de una flota y verificar que se distinguen correctamente

## 8. Endosos (spec: endosos)

- [x] 8.1 Implementar alta de endoso sobre una póliza vigente y verificar que un endoso sobre una póliza anulada es rechazado
- [x] 8.2 Implementar historial de endosos de una póliza ordenado cronológicamente y verificar el orden con varios endosos de prueba
- [x] 8.3 Implementar anulación de un endoso mal cargado y verificar que queda visible en el historial marcado como anulado, sin borrarse

## 9. Vencimientos y alertas (spec: vencimientos)

- [x] 9.1 Implementar el cálculo de vencimiento de una póliza a partir de su fecha de fin de vigencia y verificar que se actualiza al editar la póliza
- [x] 9.2 Implementar la ventana configurable de alerta (ej. 30/15/7 días) y verificar que una póliza dentro de la ventana aparece en el listado de alertas
- [x] 9.3 Implementar el listado de vencimientos filtrable por productor, compañía y ramo, y verificar el filtrado por compañía
- [x] 9.4 Implementar la acción de marcar un vencimiento como gestionado y verificar que una póliza renovada deja de listarse como pendiente

## 10. Importación de datos desde XLS y PDF (spec: importacion-datos)

- [x] 10.1 Implementar la carga y lectura de un archivo XLS como listado tabular y verificar con un archivo de ejemplo que las filas se muestran para revisión
- [x] 10.2 Implementar la extracción de listados desde archivos PDF y verificar el caso de un PDF sin datos reconocibles (debe informarse sin importar nada)
- [x] 10.3 Implementar el mapeo manual de columnas del archivo a los campos del sistema y verificar que una columna no reconocida automáticamente puede mapearse a mano
- [x] 10.4 Implementar la vista previa editable antes de confirmar la importación, incluyendo la detección de posibles duplicados contra asegurados/pólizas existentes
- [x] 10.5 Implementar el reporte final de importación (filas importadas vs. filas con error) y verificar el caso de una importación con errores parciales

## 11. Facturación y comisiones (spec: facturacion-comisiones)

- [x] 11.1 Implementar el registro de porcentaje de comisión por compañía y ramo y verificar que se usa correctamente en el cálculo
- [x] 11.2 Implementar el cálculo automático de comisión al emitir/renovar una póliza y verificar el caso de compañía/ramo sin porcentaje configurado (debe quedar pendiente, no fallar)
- [x] 11.3 Implementar la generación de liquidación de comisiones por productor y período, verificando que totaliza correctamente las pólizas del período
- [x] 11.4 Implementar la consulta del historial de liquidaciones generadas y verificar que muestra período y monto total de cada una

## 12. Backup y despliegue

- [x] 12.1 Implementar una función de backup manual del archivo de base de datos SQLite (copiarlo a una carpeta indicada por el usuario) y verificar que el archivo copiado es una base de datos válida y restaurable
- [x] 12.2 Documentar el procedimiento de instalación en una PC nueva (copiar carpeta/ejecutable, primer arranque crea la base vacía) y verificar siguiendo los pasos en una máquina limpia o entorno equivalente

## 13. Verificación integral

- [x] 13.1 Probar el flujo completo de punta a punta: alta de productor → alta de compañía → alta de asegurado → alta de póliza (incluyendo una de flota) → endoso → aparición en alertas de vencimiento → liquidación de comisión, verificando que cada paso refleja correctamente lo cargado en los anteriores
- [x] 13.2 Ejecutar `openspec validate gestion-integral-seguros --strict` y verificar que la change sigue siendo válida antes de archivarla
