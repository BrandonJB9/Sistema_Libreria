using System.Data.SqlClient;

namespace Datos
{
    /// <summary>Uses the application's shared connection configuration.</summary>
    internal static class Conexion
    {
        public static SqlConnection Crear()
        {
            return new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones());
        }
    }
}
