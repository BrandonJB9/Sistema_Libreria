using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Guarda y consulta encabezados y detalles de pedidos.</summary>
    public class PedidosDatos
    {
        /// <summary>Guarda únicamente el encabezado y devuelve su ID generado.</summary>
        public int AgregarPedido(Pedido pedido)
        {
            const string consulta = @"
                INSERT INTO dbo.Tbl_Pedidos
                    (IDCodigoSucursal, IDCliente, TipoPedido, FechaPedido, Estado)
                VALUES
                    (@IDCodigoSucursal, @IDCliente, @TipoPedido, @FechaPedido, @Estado);
                SELECT CONVERT(int, SCOPE_IDENTITY());";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                {
                    conexion.Open();
                    using (SqlTransaction transaccion = conexion.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand comando = new SqlCommand(consulta, conexion, transaccion))
                            {
                                comando.Parameters.Add("@IDCodigoSucursal", SqlDbType.Int).Value = pedido.IDCodigoSucursal;
                                comando.Parameters.Add("@IDCliente", SqlDbType.Int).Value = pedido.IDCliente;
                                comando.Parameters.Add("@TipoPedido", SqlDbType.VarChar, 100).Value = pedido.TipoPedido;
                                comando.Parameters.Add("@FechaPedido", SqlDbType.DateTime).Value = pedido.FechaPedido;
                                comando.Parameters.Add("@Estado", SqlDbType.Bit).Value = pedido.Estado;
                                int idPedido = Convert.ToInt32(comando.ExecuteScalar());
                                transaccion.Commit();
                                return idPedido;
                            }
                        }
                        catch
                        {
                            try { transaccion.Rollback(); }
                            catch { }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo guardar el encabezado del pedido.", ex);
            }
        }

        /// <summary>Guarda una línea ligada al encabezado que ya existe.</summary>
        public int AgregarDetallePedido(DetallePedido detalle)
        {
            const string consulta = @"
                INSERT INTO dbo.Tbl_DetallePedido
                    (IDPedido, Cantidad, Precio, Observaciones, Total)
                VALUES
                    (@IDPedido, @Cantidad, @Precio, @Observaciones, @Total);
                SELECT CONVERT(int, SCOPE_IDENTITY());";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                {
                    conexion.Open();
                    using (SqlTransaction transaccion = conexion.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand comando = new SqlCommand(consulta, conexion, transaccion))
                            {
                                comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = detalle.IDPedido;
                                comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = detalle.Cantidad;
                                SqlParameter precio = comando.Parameters.Add("@Precio", SqlDbType.Decimal);
                                precio.Precision = 18;
                                precio.Scale = 0;
                                precio.Value = detalle.Precio;
                                comando.Parameters.Add("@Observaciones", SqlDbType.VarChar, 100).Value =
                                    string.IsNullOrWhiteSpace(detalle.Observaciones)
                                        ? (object)DBNull.Value
                                        : detalle.Observaciones.Trim();
                                SqlParameter total = comando.Parameters.Add("@Total", SqlDbType.Decimal);
                                total.Precision = 18;
                                total.Scale = 0;
                                total.Value = detalle.Total;
                                int idDetalle = Convert.ToInt32(comando.ExecuteScalar());
                                transaccion.Commit();
                                return idDetalle;
                            }
                        }
                        catch
                        {
                            try
                            {
                                transaccion.Rollback();
                            }
                            catch
                            {
                                // Se conserva la excepción original si SQL Server también rechaza el rollback.
                            }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo guardar el detalle del pedido.", ex);
            }
        }

        public List<Pedido> ConsultarPedidos()
        {
            const string consulta = @"
                SELECT IDPedido, IDCodigoSucursal, IDCliente, TipoPedido, FechaPedido, Estado
                FROM dbo.Tbl_Pedidos
                ORDER BY IDPedido;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();
                    List<Pedido> pedidos = new List<Pedido>();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            pedidos.Add(new Pedido
                            {
                                IDPedido = lector.GetInt32(0),
                                IDCodigoSucursal = lector.GetInt32(1),
                                IDCliente = lector.GetInt32(2),
                                TipoPedido = lector.GetString(3),
                                FechaPedido = lector.GetDateTime(4),
                                Estado = lector.GetBoolean(5)
                            });
                        }
                    }
                    return pedidos;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron consultar los pedidos.", ex);
            }
        }

        public List<Pedido> BuscarPedidos(string criterio)
        {
            const string consulta = @"SELECT IDPedido, IDCodigoSucursal, IDCliente, TipoPedido, FechaPedido, Estado
                FROM dbo.Tbl_Pedidos
                WHERE CONVERT(varchar(20), IDPedido) LIKE @Criterio
                   OR CONVERT(varchar(20), IDCodigoSucursal) LIKE @Criterio
                   OR CONVERT(varchar(20), IDCliente) LIKE @Criterio
                   OR TipoPedido LIKE @Criterio
                ORDER BY IDPedido;";
            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@Criterio", SqlDbType.VarChar, 100).Value = "%" + criterio.Trim() + "%";
                    conexion.Open();
                    List<Pedido> resultado = new List<Pedido>();
                    using (SqlDataReader lector = comando.ExecuteReader())
                        while (lector.Read()) resultado.Add(new Pedido { IDPedido = lector.GetInt32(0), IDCodigoSucursal = lector.GetInt32(1), IDCliente = lector.GetInt32(2), TipoPedido = lector.GetString(3), FechaPedido = lector.GetDateTime(4), Estado = lector.GetBoolean(5) });
                    return resultado;
                }
            }
            catch (Exception ex) { throw new InvalidOperationException("No se pudieron buscar los pedidos.", ex); }
        }

        public bool EditarPedido(Pedido pedido)
        {
            const string consulta = @"UPDATE dbo.Tbl_Pedidos SET IDCodigoSucursal=@IDCodigoSucursal, IDCliente=@IDCliente,
                TipoPedido=@TipoPedido, FechaPedido=@FechaPedido, Estado=@Estado WHERE IDPedido=@IDPedido;";
            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = pedido.IDPedido;
                    comando.Parameters.Add("@IDCodigoSucursal", SqlDbType.Int).Value = pedido.IDCodigoSucursal;
                    comando.Parameters.Add("@IDCliente", SqlDbType.Int).Value = pedido.IDCliente;
                    comando.Parameters.Add("@TipoPedido", SqlDbType.VarChar, 100).Value = pedido.TipoPedido;
                    comando.Parameters.Add("@FechaPedido", SqlDbType.DateTime).Value = pedido.FechaPedido;
                    comando.Parameters.Add("@Estado", SqlDbType.Bit).Value = pedido.Estado;
                    conexion.Open(); return comando.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex) { throw new InvalidOperationException("No se pudo editar el pedido.", ex); }
        }

        public bool EliminarPedido(int idPedido)
        {
            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                {
                    conexion.Open();
                    using (SqlTransaction transaccion = conexion.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand detalle = new SqlCommand("DELETE FROM dbo.Tbl_DetallePedido WHERE IDPedido=@IDPedido;", conexion, transaccion))
                            { detalle.Parameters.Add("@IDPedido", SqlDbType.Int).Value = idPedido; detalle.ExecuteNonQuery(); }
                            int filas;
                            using (SqlCommand pedido = new SqlCommand("DELETE FROM dbo.Tbl_Pedidos WHERE IDPedido=@IDPedido;", conexion, transaccion))
                            { pedido.Parameters.Add("@IDPedido", SqlDbType.Int).Value = idPedido; filas = pedido.ExecuteNonQuery(); }
                            if (filas != 1) throw new InvalidOperationException("El pedido seleccionado ya no existe.");
                            transaccion.Commit(); return true;
                        }
                        catch { try { transaccion.Rollback(); } catch { } throw; }
                    }
                }
            }
            catch (Exception ex) { throw new InvalidOperationException("No se pudo eliminar el pedido. Verifica que no tenga otros registros relacionados.", ex); }
        }

        public List<DetallePedido> ConsultarDetallesPorPedido(int idPedido)
        {
            const string consulta = @"
                SELECT IDDetallePedido, IDPedido, Cantidad, Precio, Observaciones, Total
                FROM dbo.Tbl_DetallePedido
                WHERE IDPedido = @IDPedido
                ORDER BY IDDetallePedido;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = idPedido;
                    conexion.Open();
                    List<DetallePedido> detalles = new List<DetallePedido>();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            detalles.Add(new DetallePedido
                            {
                                IDDetallePedido = lector.GetInt32(0),
                                IDPedido = lector.GetInt32(1),
                                Cantidad = lector.GetInt32(2),
                                Precio = lector.GetDecimal(3),
                                Observaciones = lector.IsDBNull(4) ? string.Empty : lector.GetString(4),
                                Total = lector.GetDecimal(5)
                            });
                        }
                    }
                    return detalles;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron consultar los detalles del pedido.", ex);
            }
        }

        public List<DetallePedido> BuscarDetallesPorPedido(int idPedido, string criterio)
        {
            const string consulta = @"SELECT IDDetallePedido, IDPedido, Cantidad, Precio, Observaciones, Total
                FROM dbo.Tbl_DetallePedido WHERE IDPedido=@IDPedido AND
                (CONVERT(varchar(20), IDDetallePedido) LIKE @Criterio OR Observaciones LIKE @Criterio)
                ORDER BY IDDetallePedido;";
            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = idPedido;
                    comando.Parameters.Add("@Criterio", SqlDbType.VarChar, 100).Value = "%" + criterio.Trim() + "%";
                    conexion.Open();
                    List<DetallePedido> resultado = new List<DetallePedido>();
                    using (SqlDataReader lector = comando.ExecuteReader())
                        while (lector.Read()) resultado.Add(new DetallePedido { IDDetallePedido = lector.GetInt32(0), IDPedido = lector.GetInt32(1), Cantidad = lector.GetInt32(2), Precio = lector.GetDecimal(3), Observaciones = lector.IsDBNull(4) ? string.Empty : lector.GetString(4), Total = lector.GetDecimal(5) });
                    return resultado;
                }
            }
            catch (Exception ex) { throw new InvalidOperationException("No se pudieron buscar los detalles del pedido.", ex); }
        }

        public bool EditarDetallePedido(DetallePedido detalle)
        {
            const string consulta = @"UPDATE dbo.Tbl_DetallePedido SET Cantidad=@Cantidad, Precio=@Precio,
                Observaciones=@Observaciones, Total=@Total WHERE IDDetallePedido=@IDDetallePedido AND IDPedido=@IDPedido;";
            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDDetallePedido", SqlDbType.Int).Value = detalle.IDDetallePedido;
                    comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = detalle.IDPedido;
                    comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = detalle.Cantidad;
                    SqlParameter precio = comando.Parameters.Add("@Precio", SqlDbType.Decimal); precio.Precision = 18; precio.Scale = 0; precio.Value = detalle.Precio;
                    comando.Parameters.Add("@Observaciones", SqlDbType.VarChar, 100).Value = string.IsNullOrWhiteSpace(detalle.Observaciones) ? (object)DBNull.Value : detalle.Observaciones.Trim();
                    SqlParameter total = comando.Parameters.Add("@Total", SqlDbType.Decimal); total.Precision = 18; total.Scale = 0; total.Value = detalle.Total;
                    conexion.Open(); return comando.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex) { throw new InvalidOperationException("No se pudo editar el detalle del pedido.", ex); }
        }

        public bool EliminarDetallePedido(int idPedido, int idDetalle)
        {
            const string consulta = "DELETE FROM dbo.Tbl_DetallePedido WHERE IDPedido=@IDPedido AND IDDetallePedido=@IDDetallePedido;";
            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = idPedido;
                    comando.Parameters.Add("@IDDetallePedido", SqlDbType.Int).Value = idDetalle;
                    conexion.Open(); return comando.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex) { throw new InvalidOperationException("No se pudo eliminar el detalle del pedido.", ex); }
        }
    }
}
