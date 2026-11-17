using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Consulta y modifica pagos mediante ADO.NET parametrizado.</summary>
    public class PagoDatos
    {
        private const string Columnas = @"SELECT pg.IDagos, pg.TipoPago, pg.TipoServicio, pg.Comprobante,
                                                pg.IDPedido, pg.IDPrestamo, pg.IDDevoluciones,
                                                pe.TipoPedido, pr.FechaPrestamo, dv.FechaDevolucion
                                         FROM dbo.Tbl_Pagos AS pg
                                         INNER JOIN dbo.Tbl_Pedidos AS pe ON pe.IDPedido = pg.IDPedido
                                         INNER JOIN dbo.Tbl_Prestamo AS pr ON pr.IDPrestamo = pg.IDPrestamo
                                         INNER JOIN dbo.Tbl_Devoluciones AS dv ON dv.IDDevoluciones = pg.IDDevoluciones ";

        public List<Pago> Consultar()
        {
            return EjecutarConsulta(Columnas + " ORDER BY pg.IDagos;", null, null);
        }

        public List<Pago> Buscar(string criterio)
        {
            int codigo;
            bool esCodigo = int.TryParse(criterio, out codigo);
            const string filtro = @"WHERE (@Codigo IS NOT NULL AND
                                          (pg.IDagos = @Codigo OR pg.IDPedido = @Codigo
                                           OR pg.IDPrestamo = @Codigo OR pg.IDDevoluciones = @Codigo))
                                      OR pg.TipoPago LIKE @Texto OR pg.TipoServicio LIKE @Texto
                                      OR pg.Comprobante LIKE @Texto OR pe.TipoPedido LIKE @Texto";
            return EjecutarConsulta(Columnas + filtro + " ORDER BY pg.IDagos;",
                esCodigo ? (object)codigo : DBNull.Value, "%" + (criterio ?? string.Empty).Trim() + "%");
        }

        public List<KeyValuePair<int, string>> ConsultarPrestamosRelacionados()
        {
            const string consulta = @"SELECT pr.IDPrestamo, pr.FechaPrestamo, cl.NombreCliente
                                     FROM dbo.Tbl_Prestamo AS pr
                                     INNER JOIN dbo.Tbl_Cliente AS cl ON cl.IDCliente = pr.IDCliente
                                     ORDER BY pr.IDPrestamo DESC;";
            var opciones = new List<KeyValuePair<int, string>>();
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
                            string descripcion = "Préstamo " + lector.GetInt32(0) + " - " +
                                lector.GetString(2) + " - " + lector.GetDateTime(1).ToString("d");
                            opciones.Add(new KeyValuePair<int, string>(lector.GetInt32(0), descripcion));
                        }
                    }
                }
                return opciones;
            }
            catch (Exception ex) { throw new Exception("No se pudieron cargar los préstamos relacionados.", ex); }
        }

        public List<KeyValuePair<int, string>> ConsultarDevolucionesRelacionadas()
        {
            const string consulta = @"SELECT dv.IDDevoluciones, dv.FechaDevolucion, pr.IDPrestamo, cl.NombreCliente
                                     FROM dbo.Tbl_Devoluciones AS dv
                                     INNER JOIN dbo.Tbl_Prestamo AS pr ON pr.IDPrestamo = dv.IDPrestamo
                                     INNER JOIN dbo.Tbl_Cliente AS cl ON cl.IDCliente = pr.IDCliente
                                     ORDER BY dv.FechaDevolucion DESC, dv.IDDevoluciones DESC;";
            var opciones = new List<KeyValuePair<int, string>>();
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
                            string descripcion = "Devolución " + lector.GetInt32(0) + " - " +
                                lector.GetString(3) + " - " + lector.GetDateTime(1).ToString("d") +
                                " (Préstamo " + lector.GetInt32(2) + ")";
                            opciones.Add(new KeyValuePair<int, string>(lector.GetInt32(0), descripcion));
                        }
                    }
                }
                return opciones;
            }
            catch (Exception ex) { throw new Exception("No se pudieron cargar las devoluciones relacionadas.", ex); }
        }

        public int Agregar(Pago pago)
        {
            const string consulta = @"INSERT INTO dbo.Tbl_Pagos
                                         (TipoPago, TipoServicio, Comprobante, IDPedido, IDPrestamo, IDDevoluciones)
                                      OUTPUT INSERTED.IDagos
                                      VALUES (@TipoPago, @TipoServicio, @Comprobante, @IDPedido, @IDPrestamo, @IDDevoluciones);";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametros(comando, pago);
                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo agregar el pago.", ex);
            }
        }

        public bool Editar(Pago pago)
        {
            const string consulta = @"UPDATE dbo.Tbl_Pagos
                                      SET TipoPago = @TipoPago, TipoServicio = @TipoServicio,
                                          Comprobante = @Comprobante, IDPedido = @IDPedido,
                                          IDPrestamo = @IDPrestamo, IDDevoluciones = @IDDevoluciones
                                      WHERE IDagos = @IDagos;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametros(comando, pago);
                    comando.Parameters.Add("@IDagos", SqlDbType.Int).Value = pago.IDagos;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo actualizar el pago.", ex);
            }
        }

        public bool Eliminar(int idPago)
        {
            const string consulta = "DELETE FROM dbo.Tbl_Pagos WHERE IDagos = @IDagos;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDagos", SqlDbType.Int).Value = idPago;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo eliminar el pago.", ex);
            }
        }

        public bool ExistenRelaciones(Pago pago)
        {
            const string consulta = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Tbl_Pedidos WHERE IDPedido = @IDPedido)
                                                  AND EXISTS (SELECT 1 FROM dbo.Tbl_Prestamo WHERE IDPrestamo = @IDPrestamo)
                                                  AND EXISTS (SELECT 1 FROM dbo.Tbl_Devoluciones
                                                              WHERE IDDevoluciones = @IDDevoluciones
                                                                AND IDPrestamo = @IDPrestamo)
                                             THEN 1 ELSE 0 END;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = pago.IDPedido;
                    comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = pago.IDPrestamo;
                    comando.Parameters.Add("@IDDevoluciones", SqlDbType.Int).Value = pago.IDDevoluciones;
                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar()) == 1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron validar las relaciones del pago.", ex);
            }
        }

        private static List<Pago> EjecutarConsulta(string consulta, object codigo, object texto)
        {
            var pagos = new List<Pago>();
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    if (codigo != null)
                    {
                        comando.Parameters.Add("@Codigo", SqlDbType.Int).Value = codigo;
                        comando.Parameters.Add("@Texto", SqlDbType.VarChar, 152).Value = texto;
                    }

                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            pagos.Add(new Pago
                            {
                                IDagos = lector.GetInt32(lector.GetOrdinal("IDagos")),
                                TipoPago = lector.GetString(lector.GetOrdinal("TipoPago")),
                                TipoServicio = lector.GetString(lector.GetOrdinal("TipoServicio")),
                                Comprobante = lector.GetString(lector.GetOrdinal("Comprobante")),
                                IDPedido = lector.GetInt32(lector.GetOrdinal("IDPedido")),
                                IDPrestamo = lector.GetInt32(lector.GetOrdinal("IDPrestamo")),
                                IDDevoluciones = lector.GetInt32(lector.GetOrdinal("IDDevoluciones")),
                                TipoPedido = lector.GetString(lector.GetOrdinal("TipoPedido")),
                                FechaPrestamo = lector.GetDateTime(lector.GetOrdinal("FechaPrestamo")),
                                FechaDevolucion = lector.GetDateTime(lector.GetOrdinal("FechaDevolucion"))
                            });
                        }
                    }
                }
                return pagos;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron consultar los pagos.", ex);
            }
        }

        private static void AgregarParametros(SqlCommand comando, Pago pago)
        {
            comando.Parameters.Add("@TipoPago", SqlDbType.VarChar, 50).Value = pago.TipoPago;
            comando.Parameters.Add("@TipoServicio", SqlDbType.VarChar, 50).Value = pago.TipoServicio;
            comando.Parameters.Add("@Comprobante", SqlDbType.VarChar, 50).Value = pago.Comprobante;
            comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = pago.IDPedido;
            comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = pago.IDPrestamo;
            comando.Parameters.Add("@IDDevoluciones", SqlDbType.Int).Value = pago.IDDevoluciones;
        }
    }
}
