# 05 — Conexión a SQL Server (guía de referencia)

**Estado: guía completada y conexión probada con `System.Data.SqlClient`; se conserva como referencia.** La prueba previa de `sqlcmd`/ODBC 18 desde Windows falló por cifrado.

## Objetivo

Documentar cómo preparar y comprobar una conexión desde C# con .NET Framework y Windows Forms, manteniendo la futura solución organizada por capas.

## Datos proporcionados

El estudiante confirmó que el árbol creado es correcto y proporcionó estos datos para la conexión:

- Servidor: `localhost`
- Puerto: `1434`
- Base de datos: `libreriaProyecto`
- Autenticación: SQL Server
- Usuario: `sa`

El servidor, puerto y base fueron confirmados con una consulta dentro del contenedor. La contraseña no se copia a este documento, al código ni a archivos versionados.

## Responsabilidad por capas

- **Entidad:** clases sencillas para representar datos del dominio.
- **Datos:** abrir conexiones y ejecutar comandos SQL.
- **Negocio:** reglas y validaciones del sistema.
- **Presentación:** formularios Windows Forms; no poner consultas SQL directamente en los eventos de la interfaz.

## Cadena de conexión

Para SQL Server con puerto explícito, el valor de `Data Source` usa `servidor,puerto`:

```text
Data Source=localhost,1434;Initial Catalog=libreriaProyecto;User ID=sa;Password=<obtener-seguro>;Encrypt=True;TrustServerCertificate=True;
```

`<obtener-seguro>` es un marcador, no una contraseña válida. No reemplazarlo con una contraseña en un archivo que se vaya a guardar en Git. En un entorno de desarrollo, cargar el secreto desde una configuración local excluida del control de versiones o desde un almacén de secretos disponible en el equipo. No publicar ni compartir ese archivo.

## Ejemplo didáctico en la capa Datos

El siguiente ejemplo muestra la forma de abrir y cerrar una conexión. Requiere `using System.Data.SqlClient;`. No debe copiarse una contraseña real al código fuente.

```csharp
using System;
using System.Data.SqlClient;

public class ConexionBD
{
    private readonly string cadenaConexion;

    public ConexionBD(string cadenaConexionSegura)
    {
        cadenaConexion = cadenaConexionSegura;
    }

    public bool ProbarConexion()
    {
        try
        {
            // using garantiza que la conexión se cierre al terminar.
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                return true;
            }
        }
        catch (SqlException ex)
        {
            // En una aplicación real, registrar el detalle de forma segura
            // y mostrar al usuario un mensaje que no revele datos sensibles.
            throw new InvalidOperationException("No fue posible conectar con la base de datos.", ex);
        }
    }
}
```

El formulario no debe mostrar la contraseña ni concatenar valores ingresados por el usuario dentro de SQL. En los CRUD, usar `SqlParameter` para todos los valores variables.

## Comprobación cuando se autorice

1. Confirmar que SQL Server está instalado y que la instancia acepta conexiones TCP/IP en el puerto configurado.
2. Confirmar que `libreriaProyecto` existe y que el usuario tiene solo los permisos necesarios para el trabajo.
3. Obtener la contraseña desde el mecanismo local seguro; no pedirla ni imprimirla en consola o mensajes.
4. Probar primero desde una utilidad o método de la capa Datos y capturar fallos con `try-catch`.
5. Registrar el resultado observado (éxito o mensaje técnico sin secretos). No marcar la conexión como validada antes de realizar esa prueba.

## Estado de validación

El estudiante confirmó el árbol real. `sqlcmd` de Windows es la versión 17.0.1000.7 con Microsoft ODBC Driver 18. El servicio local `MSSQL$SQLEXPRESS` está en ejecución y `SQLBrowser` está detenido. El contenedor Docker `sql_prog2b`, basado en `mcr.microsoft.com/mssql/server:2022-latest`, publica TCP 1434 del host hacia TCP 1433 del contenedor.

La conexión desde el cliente Windows falló incluso con `-No -C`, con los mensajes `Encryption not supported on the client` y `No hay credenciales disponibles en el paquete de seguridad`. Dentro del contenedor, `sqlcmd -C` sí conectó al SQL Server confiando en el certificado autofirmado.

Las consultas se ejecutaron interactivamente dentro del contenedor. Sin seleccionar una base, `DB_NAME()` devolvió `master`; las primeras cinco tablas en ese contexto fueron `dbo.MSreplication_options`, `dbo.spt_fallback_db`, `dbo.spt_fallback_dev`, `dbo.spt_fallback_usg` y `dbo.spt_monitor`. Después de seleccionar `libreriaProyecto`, `DB_NAME()` devolvió `libreriaProyecto`; sus primeras cinco tablas por nombre fueron `dbo.Tbl_Autor`, `dbo.Tbl_Categoria`, `dbo.Tbl_Cliente`, `dbo.Tbl_DetalleDevolucion` y `dbo.Tbl_DetallePedido`.

No se ejecutaron scripts de modificación ni se cambiaron datos.
