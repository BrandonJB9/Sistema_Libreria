using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Consulta los pedidos padre disponibles para relacionarlos con libros.</summary>
    public class PedidoDatos
    {
        public List<Pedido> Consultar()
        {
            const string consulta = "SELECT IDPedido, TipoPedido FROM dbo.Tbl_Pedidos ORDER BY IDPedido;";
            var pedidos = new List<Pedido>();
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            pedidos.Add(new Pedido
                            {
                                IDPedido = lector.GetInt32(0),
                                TipoPedido = lector.GetString(1)
                            });
                        }
                    }
                }
                return pedidos;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron consultar los pedidos.", ex);
            }
        }
    }
}
