using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Consulta encabezados de préstamo desde SQL Server.</summary>
    public class PrestamoDatos
    {
        /// <summary>Elimina encabezado y detalles juntos; conserva préstamos con devoluciones o pagos.</summary>
        public void EliminarConDetalles(int idPrestamo)
        {
            using (SqlConnection conexion = Conexion.Crear())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        const string relacionados = @"SELECT CASE WHEN
                            EXISTS (SELECT 1 FROM dbo.Tbl_Devoluciones WHERE IDPrestamo = @ID)
                            OR EXISTS (SELECT 1 FROM dbo.Tbl_Pagos WHERE IDPrestamo = @ID)
                            THEN 1 ELSE 0 END;";
                        using (var comando = new SqlCommand(relacionados, conexion, transaccion))
                        {
                            comando.Parameters.Add("@ID", SqlDbType.Int).Value = idPrestamo;
                            if (Convert.ToInt32(comando.ExecuteScalar()) != 0)
                                throw new InvalidOperationException("El préstamo tiene devoluciones o pagos relacionados y no se puede eliminar.");
                        }
                        using (var comando = new SqlCommand("DELETE FROM dbo.Tbl_DetallePrestamo WHERE IDPrestamo = @ID;", conexion, transaccion))
                        {
                            comando.Parameters.Add("@ID", SqlDbType.Int).Value = idPrestamo;
                            comando.ExecuteNonQuery();
                        }
                        using (var comando = new SqlCommand("DELETE FROM dbo.Tbl_Prestamo WHERE IDPrestamo = @ID;", conexion, transaccion))
                        {
                            comando.Parameters.Add("@ID", SqlDbType.Int).Value = idPrestamo;
                            if (comando.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException("El préstamo seleccionado ya no existe.");
                        }
                        transaccion.Commit();
                    }
                    catch
                    {
                        try { transaccion.Rollback(); } catch { /* Se conserva el error original. */ }
                        throw;
                    }
                }
            }
        }

        public void Actualizar(Prestamo prestamo)
        {
            const string consulta = @"UPDATE dbo.Tbl_Prestamo
                SET IDCliente = @IDCliente, IDCodigoSucursal = @IDCodigoSucursal,
                    FechaPrestamo = @FechaPrestamo, DiasPrestamo = @DiasPrestamo,
                    Observaciones = @Observaciones, Estado = @Estado
                WHERE IDPrestamo = @IDPrestamo;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = prestamo.IDPrestamo;
                    comando.Parameters.Add("@IDCliente", SqlDbType.Int).Value = prestamo.IDCliente;
                    comando.Parameters.Add("@IDCodigoSucursal", SqlDbType.Int).Value = prestamo.IDCodigoSucursal;
                    comando.Parameters.Add("@FechaPrestamo", SqlDbType.DateTime).Value = prestamo.FechaPrestamo;
                    comando.Parameters.Add("@DiasPrestamo", SqlDbType.Int).Value = prestamo.DiasPrestamo;
                    comando.Parameters.Add("@Observaciones", SqlDbType.VarChar, 50).Value =
                        (object)prestamo.Observaciones ?? DBNull.Value;
                    comando.Parameters.Add("@Estado", SqlDbType.Bit).Value = prestamo.Estado;
                    conexion.Open();
                    if (comando.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("No se encontró el préstamo seleccionado.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo actualizar el préstamo.", ex);
            }
        }

        public int AgregarConDetalles(Prestamo prestamo, IList<DetallePrestamo> detalles)
        {
            const string insertarPrestamo = @"INSERT INTO dbo.Tbl_Prestamo
                (IDCliente, IDCodigoSucursal, FechaPrestamo, DiasPrestamo, Observaciones, Estado)
                OUTPUT INSERTED.IDPrestamo
                VALUES (@IDCliente, @IDCodigoSucursal, @FechaPrestamo, @DiasPrestamo, @Observaciones, @Estado);";
            const string insertarDetalle = @"INSERT INTO dbo.Tbl_DetallePrestamo
                (IDPrestamo, Cantidad, Precio, Descuento, Total)
                VALUES (@IDPrestamo, @Cantidad, @Precio, @Descuento, @Total);";

            SqlTransaction transaccion = null;
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                {
                    conexion.Open();
                    transaccion = conexion.BeginTransaction();
                    int idPrestamo;
                    using (SqlCommand comando = new SqlCommand(insertarPrestamo, conexion, transaccion))
                    {
                        comando.Parameters.Add("@IDCliente", SqlDbType.Int).Value = prestamo.IDCliente;
                        comando.Parameters.Add("@IDCodigoSucursal", SqlDbType.Int).Value = prestamo.IDCodigoSucursal;
                        comando.Parameters.Add("@FechaPrestamo", SqlDbType.DateTime).Value = prestamo.FechaPrestamo;
                        comando.Parameters.Add("@DiasPrestamo", SqlDbType.Int).Value = prestamo.DiasPrestamo;
                        comando.Parameters.Add("@Observaciones", SqlDbType.VarChar, 50).Value =
                            (object)prestamo.Observaciones ?? DBNull.Value;
                        comando.Parameters.Add("@Estado", SqlDbType.Bit).Value = prestamo.Estado;
                        idPrestamo = Convert.ToInt32(comando.ExecuteScalar());
                    }

                    foreach (DetallePrestamo detalle in detalles)
                    {
                        using (SqlCommand comando = new SqlCommand(insertarDetalle, conexion, transaccion))
                        {
                            comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = idPrestamo;
                            comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = detalle.Cantidad;
                            comando.Parameters.Add("@Precio", SqlDbType.Decimal).Value = detalle.Precio;
                            comando.Parameters["@Precio"].Precision = 18;
                            comando.Parameters["@Precio"].Scale = 0;
                            comando.Parameters.Add("@Descuento", SqlDbType.Decimal).Value =
                                (object)detalle.Descuento ?? DBNull.Value;
                            comando.Parameters["@Descuento"].Precision = 18;
                            comando.Parameters["@Descuento"].Scale = 0;
                            comando.Parameters.Add("@Total", SqlDbType.Decimal).Value = detalle.Total;
                            comando.Parameters["@Total"].Precision = 18;
                            comando.Parameters["@Total"].Scale = 0;
                            comando.ExecuteNonQuery();
                        }
                    }

                    transaccion.Commit();
                    return idPrestamo;
                }
            }
            catch (Exception ex)
            {
                if (transaccion != null)
                {
                    try { transaccion.Rollback(); }
                    catch { /* Si SQL Server ya cerró la transacción, se conserva el error original. */ }
                    transaccion.Dispose();
                }
                throw new Exception("No se pudo guardar el préstamo y sus detalles; se revirtió la operación. " +
                    "Detalle: " + ex.GetBaseException().Message, ex);
            }
        }

        public List<Prestamo> Consultar()
        {
            const string consulta = @"SELECT p.IDPrestamo, p.IDCliente, p.IDCodigoSucursal, p.FechaPrestamo,
                                            p.DiasPrestamo, p.Observaciones, p.Estado,
                                            c.NombreCliente, s.NombreSucursal
                                     FROM dbo.Tbl_Prestamo AS p
                                     INNER JOIN dbo.Tbl_Cliente AS c ON c.IDCliente = p.IDCliente
                                     INNER JOIN dbo.Tbl_Sucursales AS s ON s.IDCodigoSucursal = p.IDCodigoSucursal
                                     ORDER BY p.IDPrestamo DESC;";
            var prestamos = new List<Prestamo>();
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            prestamos.Add(new Prestamo
                            {
                                IDPrestamo = lector.GetInt32(0),
                                IDCliente = lector.GetInt32(1),
                                IDCodigoSucursal = lector.GetInt32(2),
                                FechaPrestamo = lector.GetDateTime(3),
                                DiasPrestamo = lector.GetInt32(4),
                                Observaciones = lector.IsDBNull(5) ? null : lector.GetString(5),
                                Estado = lector.GetBoolean(6),
                                NombreCliente = lector.GetString(7),
                                NombreSucursal = lector.GetString(8)
                            });
                        }
                    }
                }
                return prestamos;
            }
            catch (Exception ex) { throw new Exception("No se pudieron consultar los préstamos.", ex); }
        }

        public List<KeyValuePair<int, string>> ConsultarClientesRelacionados()
        {
            return ConsultarOpciones("SELECT IDCliente, NombreCliente FROM dbo.Tbl_Cliente ORDER BY NombreCliente;");
        }

        public List<KeyValuePair<int, string>> ConsultarSucursalesRelacionadas()
        {
            return ConsultarOpciones("SELECT IDCodigoSucursal, NombreSucursal FROM dbo.Tbl_Sucursales ORDER BY NombreSucursal;");
        }

        private static List<KeyValuePair<int, string>> ConsultarOpciones(string consulta)
        {
            var opciones = new List<KeyValuePair<int, string>>();
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                            opciones.Add(new KeyValuePair<int, string>(lector.GetInt32(0),
                                lector.GetInt32(0) + " - " + lector.GetString(1)));
                    }
                }
                return opciones;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron cargar los clientes o sucursales relacionados.", ex);
            }
        }
    }
}
