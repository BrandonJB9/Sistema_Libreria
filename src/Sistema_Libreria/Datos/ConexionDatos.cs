using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public static class ConexionDatos
    {
        internal static string ObtenerCadenaConexion()
        {
            ConnectionStringSettings configuracion =
                ConfigurationManager.ConnectionStrings["LibreriaProyecto"];

            if (configuracion == null || string.IsNullOrWhiteSpace(configuracion.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    "No se encontró la cadena 'LibreriaProyecto' en la configuración de la aplicación.");
            }

            return configuracion.ConnectionString;
        }

        internal static string ObtenerCadenaParaOperaciones()
        {
            return ObtenerCadenaConexion();
        }

        /// <summary>
        /// Abre y cierra una conexión para confirmar que el servidor responde.
        /// No consulta ni modifica tablas.
        /// </summary>
        public static bool ProbarConexion(out string mensaje)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(ObtenerCadenaConexion()))
                {
                    conexion.Open();
                    mensaje = "Conexión exitosa con la base de datos LibreriaProyecto.";
                    return true;
                }
            }
            catch (SqlException ex)
            {
                mensaje = "No se pudo conectar con SQL Server. Verifica que el servicio esté activo, " +
                          "que el puerto 1434 esté disponible y que las credenciales sean correctas. " +
                          "Detalle técnico: " + ex.Message;
                return false;
            }
            catch (ConfigurationErrorsException ex)
            {
                mensaje = "La configuración de conexión está incompleta o no es válida. " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                mensaje = "Ocurrió un problema al preparar la conexión con la base de datos. " + ex.Message;
                return false;
            }
        }
    }
}
