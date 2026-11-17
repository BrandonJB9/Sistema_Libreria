using System.Data.SqlClient;

namespace Datos
{
    /// <summary>
    /// Adaptador de compatibilidad para los módulos de pedidos incorporados.
    /// Reutiliza la cadena central que ya configura el proyecto.
    /// </summary>
    public static class ConexionSqlServer
    {
        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones());
        }
    }
}
