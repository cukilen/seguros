# Instalación en una PC nueva

La aplicación es un ejecutable de Windows autocontenido: no requiere instalar
.NET, ni un servidor de base de datos, ni ninguna otra dependencia en la PC
destino.

## Pasos

1. Generar el ejecutable (se hace una sola vez, en la PC de desarrollo):

   ```powershell
   dotnet publish src\Seguros.App\Seguros.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
   ```

   Esto genera `publish\Seguros.exe`.

2. Copiar la carpeta `publish` completa a la PC destino (por USB, red local, etc.).

3. Ejecutar `Seguros.exe`. En el primer arranque, la aplicación crea
   automáticamente su base de datos vacía en:

   ```
   %LOCALAPPDATA%\Seguros\seguros.db
   ```

   (por ejemplo `C:\Users\<usuario>\AppData\Local\Seguros\seguros.db`)

4. En la pantalla de ingreso, hacer clic en **"¿Primera vez? Crear productor"**
   para dar de alta el primer productor y empezar a usar el sistema.

## Backup

Desde la pestaña **Backup** de la aplicación se puede generar en cualquier
momento una copia del archivo `seguros.db`. Se recomienda guardar esa copia
fuera de la PC (pendrive, carpeta en la nube) por si la PC falla.

## Actualizar la aplicación a una versión nueva

Reemplazar `Seguros.exe` por la versión nueva. La base de datos
(`seguros.db`) no se toca: al iniciar, la aplicación aplica automáticamente
cualquier cambio de esquema pendiente sobre los datos existentes.
