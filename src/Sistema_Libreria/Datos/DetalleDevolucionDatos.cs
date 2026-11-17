using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Consultas de lectura para el detalle de una devolución seleccionada.</summary>
    public class DetalleDevolucionDatos
    {
        public void Editar(DetalleDevolucionEntidad detalle)
        {
            const string sql = @"UPDATE dbo.Tbl_DetalleDevolucion
                                 SET Cantidad = @Cantidad, Multa = @Multa
                                 WHERE IDDetalleDaevolucion = @IDDetalleDevolucion
                                   AND IDDevoluciones = @IDDevoluciones;";
            try
            {
                using (var conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (var comando = new SqlCommand(sql, conexion))
                {
                    AgregarParametrosDetalle(comando, detalle);
                    conexion.Open();
                    if (comando.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("No se encontró el detalle dentro de la devolución seleccionada.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("No se pudo editar el detalle de la devolución. " + ex.Message, ex);
            }
        }

        public void Eliminar(int idDevolucion, int idDetalle)
        {
            const string sql = @"DELETE FROM dbo.Tbl_DetalleDevolucion
                                 WHERE IDDetalleDaevolucion = @IDDetalleDevolucion
                                   AND IDDevoluciones = @IDDevoluciones;";
            try
            {
                using (var conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add("@IDDetalleDevolucion", SqlDbType.Int).Value = idDetalle;
                    comando.Parameters.Add("@IDDevoluciones", SqlDbType.Int).Value = idDevolucion;
                    conexion.Open();
                    if (comando.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("No se encontró el detalle dentro de la devolución seleccionada.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("No se pudo eliminar el detalle de la devolución. " + ex.Message, ex);
            }
        }

        private static void AgregarParametrosDetalle(SqlCommand comando, DetalleDevolucionEntidad detalle)
        {
            comando.Parameters.Add("@IDDetalleDevolucion", SqlDbType.Int).Value = detalle.IDDetalleDevolucion;
            comando.Parameters.Add("@IDDevoluciones", SqlDbType.Int).Value = detalle.IDDevoluciones;
            comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = detalle.Cantidad;
            SqlParameter multa = comando.Parameters.Add("@Multa", SqlDbType.Decimal);
            multa.Precision = 18;
            multa.Scale = 0;
            multa.Value = (object)detalle.Multa ?? DBNull.Value;
        }

        public List<DetalleDevolucionEntidad> ConsultarPorDevolucion(int idDevolucion, string filtro)
        {
            const string sql = @"SELECT IDDetalleDaevolucion AS IDDetalleDevolucion,
                                        IDDevoluciones, Cantidad, Multa
                                 FROM dbo.Tbl_DetalleDevolucion
                                 WHERE IDDevoluciones = @IDDevoluciones
                                   AND (@Filtro = ''
                                        OR IDDetalleDaevolucion = TRY_CONVERT(int, @Filtro)
                                        OR Cantidad = TRY_CONVERT(int, @Filtro))
                                 ORDER BY IDDetalleDaevolucion DESC;";
            var detalles = new List<DetalleDevolucionEntidad>();
            try
            {
                using (var conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add("@IDDevoluciones", SqlDbType.Int).Value = idDevolucion;
                    comando.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            detalles.Add(new DetalleDevolucionEntidad
                            {
                                IDDetalleDevolucion = Convert.ToInt32(lector["IDDetalleDevolucion"]),
                                IDDevoluciones = Convert.ToInt32(lector["IDDevoluciones"]),
                                Cantidad = Convert.ToInt32(lector["Cantidad"]),
                                Multa = lector["Multa"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(lector["Multa"])
                            });
                        }
                    }
                }
                return detalles;
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("No se pudo consultar el detalle de la devolución seleccionada. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ocurrió un problema al consultar el detalle de devolución. " + ex.Message, ex);
            }
        }

        /// <summary>Obtiene totales de todo el detalle, no solo de la devolución o página visible.</summary>
        public decimal[] ConsultarTotales()
        {
            const string sql = @"SELECT ISNULL(SUM(Cantidad), 0) AS TotalLibros,
                                        ISNULL(SUM(CASE WHEN Multa > 0 THEN Cantidad ELSE 0 END), 0) AS ConMulta,
                                        ISNULL(SUM(CASE WHEN Multa IS NULL OR Multa = 0 THEN Cantidad ELSE 0 END), 0) AS SinMulta
                                 FROM dbo.Tbl_DetalleDevolucion;";
            try
            {
                using (var conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (var comando = new SqlCommand(sql, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        lector.Read();
                        return new[]
                        {
                            Convert.ToDecimal(lector["TotalLibros"]),
                            Convert.ToDecimal(lector["ConMulta"]),
                            Convert.ToDecimal(lector["SinMulta"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("No se pudieron consultar los totales del detalle de devolución. " + ex.Message, ex);
            }
        }
    }
}
