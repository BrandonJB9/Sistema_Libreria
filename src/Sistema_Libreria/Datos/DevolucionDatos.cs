using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Consultas de lectura para los encabezados de devoluciones.</summary>
    public class DevolucionDatos
    {
        /// <summary>Guarda el encabezado y todas sus filas de detalle en una sola transacción.</summary>
        public int AgregarConDetalles(DevolucionEntidad encabezado,
            IList<DetalleDevolucionEntidad> detalles)
        {
            const string sqlEncabezado = @"INSERT INTO dbo.Tbl_Devoluciones
                    (IDPrestamo, FechaDevolucion, DiasRetraso, Observaciones, RecibidoPor)
                    OUTPUT INSERTED.IDDevoluciones
                    VALUES (@IDPrestamo, @FechaDevolucion, @DiasRetraso, @Observaciones, @RecibidoPor);";
            const string sqlDetalle = @"INSERT INTO dbo.Tbl_DetalleDevolucion
                    (IDDevoluciones, Cantidad, Multa)
                    VALUES (@IDDevoluciones, @Cantidad, @Multa);";

            using (var conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        int idEncabezado;
                        using (var comando = new SqlCommand(sqlEncabezado, conexion, transaccion))
                        {
                            comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = encabezado.IDPrestamo;
                            comando.Parameters.Add("@FechaDevolucion", SqlDbType.DateTime).Value = encabezado.FechaDevolucion;
                            comando.Parameters.Add("@DiasRetraso", SqlDbType.Int).Value = (object)encabezado.DiasRetraso ?? DBNull.Value;
                            comando.Parameters.Add("@Observaciones", SqlDbType.VarChar, 55).Value = (object)encabezado.Observaciones ?? DBNull.Value;
                            comando.Parameters.Add("@RecibidoPor", SqlDbType.VarChar, 100).Value = (object)encabezado.RecibidoPor ?? DBNull.Value;
                            idEncabezado = Convert.ToInt32(comando.ExecuteScalar());
                        }

                        foreach (DetalleDevolucionEntidad detalle in detalles)
                        {
                            using (var comando = new SqlCommand(sqlDetalle, conexion, transaccion))
                            {
                                // El IDENTITY generado para el encabezado se guarda como FK en cada detalle.
                                comando.Parameters.Add("@IDDevoluciones", SqlDbType.Int).Value = idEncabezado;
                                comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = detalle.Cantidad;
                                SqlParameter multa = comando.Parameters.Add("@Multa", SqlDbType.Decimal);
                                multa.Precision = 18;
                                multa.Scale = 0;
                                multa.Value = (object)detalle.Multa ?? DBNull.Value;
                                if (comando.ExecuteNonQuery() != 1)
                                    throw new InvalidOperationException("No se pudo guardar una fila de detalle.");
                            }
                        }

                        transaccion.Commit();
                        return idEncabezado;
                    }
                    catch (Exception ex)
                    {
                        try { transaccion.Rollback(); }
                        catch (Exception errorRollback)
                        {
                            throw new ApplicationException("Falló el guardado y también el rollback de la transacción. " + errorRollback.Message, ex);
                        }
                        throw new ApplicationException("No se guardó la devolución completa; la transacción se revirtió. " + ex.Message, ex);
                    }
                }
            }
        }

        /// <summary>Actualiza los datos del encabezado sin cambiar su identidad ni sus detalles.</summary>
        public void Editar(DevolucionEntidad encabezado)
        {
            const string sql = @"UPDATE dbo.Tbl_Devoluciones
                                 SET IDPrestamo = @IDPrestamo,
                                     FechaDevolucion = @FechaDevolucion,
                                     DiasRetraso = @DiasRetraso,
                                     Observaciones = @Observaciones,
                                     RecibidoPor = @RecibidoPor
                                 WHERE IDDevoluciones = @IDDevoluciones;";
            try
            {
                using (var conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add("@IDDevoluciones", SqlDbType.Int).Value = encabezado.IDDevoluciones;
                    comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = encabezado.IDPrestamo;
                    comando.Parameters.Add("@FechaDevolucion", SqlDbType.DateTime).Value = encabezado.FechaDevolucion;
                    comando.Parameters.Add("@DiasRetraso", SqlDbType.Int).Value = (object)encabezado.DiasRetraso ?? DBNull.Value;
                    comando.Parameters.Add("@Observaciones", SqlDbType.VarChar, 55).Value = (object)encabezado.Observaciones ?? DBNull.Value;
                    comando.Parameters.Add("@RecibidoPor", SqlDbType.VarChar, 100).Value = (object)encabezado.RecibidoPor ?? DBNull.Value;
                    conexion.Open();
                    if (comando.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("No se encontró la devolución seleccionada.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("No se pudo editar el encabezado de la devolución. " + ex.Message, ex);
            }
        }

        /// <summary>Elimina primero los detalles y luego el encabezado en una sola transacción.</summary>
        public void EliminarConDetalles(int idDevolucion)
        {
            const string sqlDetalles = "DELETE FROM dbo.Tbl_DetalleDevolucion WHERE IDDevoluciones = @IDDevoluciones;";
            const string sqlEncabezado = "DELETE FROM dbo.Tbl_Devoluciones WHERE IDDevoluciones = @IDDevoluciones;";
            using (var conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        using (var comandoDetalles = new SqlCommand(sqlDetalles, conexion, transaccion))
                        {
                            comandoDetalles.Parameters.Add("@IDDevoluciones", SqlDbType.Int).Value = idDevolucion;
                            comandoDetalles.ExecuteNonQuery();
                        }

                        using (var comandoEncabezado = new SqlCommand(sqlEncabezado, conexion, transaccion))
                        {
                            comandoEncabezado.Parameters.Add("@IDDevoluciones", SqlDbType.Int).Value = idDevolucion;
                            if (comandoEncabezado.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException("No se encontró la devolución seleccionada.");
                        }

                        transaccion.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { transaccion.Rollback(); }
                        catch (Exception errorRollback)
                        {
                            throw new ApplicationException("Falló la eliminación y también el rollback. " + errorRollback.Message, ex);
                        }
                        throw new ApplicationException("No se eliminó la devolución; sus detalles se conservaron. " + ex.Message, ex);
                    }
                }
            }
        }

        public List<DevolucionEntidad> Consultar(string filtro)
        {
            const string sql = @"SELECT IDDevoluciones, IDPrestamo, FechaDevolucion,
                                        DiasRetraso, Observaciones, RecibidoPor
                                 FROM dbo.Tbl_Devoluciones
                                 WHERE (@Filtro = ''
                                        OR IDDevoluciones = TRY_CONVERT(int, @Filtro)
                                        OR IDPrestamo = TRY_CONVERT(int, @Filtro)
                                        OR ISNULL(RecibidoPor, '') LIKE '%' + @Filtro + '%'
                                        OR ISNULL(Observaciones, '') LIKE '%' + @Filtro + '%')
                                 ORDER BY IDDevoluciones DESC;";
            var devoluciones = new List<DevolucionEntidad>();
            try
            {
                using (var conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            devoluciones.Add(new DevolucionEntidad
                            {
                                IDDevoluciones = Convert.ToInt32(lector["IDDevoluciones"]),
                                IDPrestamo = Convert.ToInt32(lector["IDPrestamo"]),
                                FechaDevolucion = Convert.ToDateTime(lector["FechaDevolucion"]),
                                DiasRetraso = lector["DiasRetraso"] == DBNull.Value ? (int?)null : Convert.ToInt32(lector["DiasRetraso"]),
                                Observaciones = lector["Observaciones"] == DBNull.Value ? null : Convert.ToString(lector["Observaciones"]),
                                RecibidoPor = lector["RecibidoPor"] == DBNull.Value ? null : Convert.ToString(lector["RecibidoPor"])
                            });
                        }
                    }
                }
                return devoluciones;
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("No se pudieron consultar los encabezados de devolución. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ocurrió un problema al consultar los encabezados de devolución. " + ex.Message, ex);
            }
        }

        /// <summary>Obtiene totales globales; no depende del filtro ni de la página visible.</summary>
        public int[] ConsultarTotales()
        {
            const string sql = @"SELECT COUNT(*) AS Total,
                                        SUM(CASE WHEN DiasRetraso > 0 THEN 1 ELSE 0 END) AS ConRetraso,
                                        SUM(CASE WHEN DiasRetraso = 0 THEN 1 ELSE 0 END) AS AlTiempo
                                 FROM dbo.Tbl_Devoluciones;";
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
                            Convert.ToInt32(lector["Total"]),
                            lector["ConRetraso"] == DBNull.Value ? 0 : Convert.ToInt32(lector["ConRetraso"]),
                            lector["AlTiempo"] == DBNull.Value ? 0 : Convert.ToInt32(lector["AlTiempo"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("No se pudieron consultar los totales de devolución. " + ex.Message, ex);
            }
        }
    }
}
