using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Consulta filas detalle asociadas a un encabezado.</summary>
    public class DetallePrestamoDatos
    {
        public int Agregar(DetallePrestamo detalle)
        {
            const string consulta = @"INSERT INTO dbo.Tbl_DetallePrestamo
                (IDPrestamo, Cantidad, Precio, Descuento, Total)
                OUTPUT INSERTED.IDDetallePrestamo
                VALUES (@IDPrestamo, @Cantidad, @Precio, @Descuento, @Total);";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = detalle.IDPrestamo;
                    comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = detalle.Cantidad;
                    AgregarDecimal(comando, "@Precio", detalle.Precio);
                    AgregarDecimal(comando, "@Descuento", (object)detalle.Descuento ?? DBNull.Value);
                    AgregarDecimal(comando, "@Total", detalle.Total);
                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo agregar el detalle del préstamo.", ex);
            }
        }

        public void Actualizar(DetallePrestamo detalle)
        {
            const string consulta = @"UPDATE dbo.Tbl_DetallePrestamo
                SET Cantidad = @Cantidad, Precio = @Precio, Descuento = @Descuento, Total = @Total
                WHERE IDDetallePrestamo = @IDDetallePrestamo AND IDPrestamo = @IDPrestamo;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDDetallePrestamo", SqlDbType.Int).Value = detalle.IDDetallePrestamo;
                    comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = detalle.IDPrestamo;
                    comando.Parameters.Add("@Cantidad", SqlDbType.Int).Value = detalle.Cantidad;
                    AgregarDecimal(comando, "@Precio", detalle.Precio);
                    AgregarDecimal(comando, "@Descuento", (object)detalle.Descuento ?? DBNull.Value);
                    AgregarDecimal(comando, "@Total", detalle.Total);

                    conexion.Open();
                    if (comando.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("No se encontró ese detalle dentro del préstamo seleccionado.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo actualizar el detalle del préstamo.", ex);
            }
        }

        private static void AgregarDecimal(SqlCommand comando, string nombre, object valor)
        {
            SqlParameter parametro = comando.Parameters.Add(nombre, SqlDbType.Decimal);
            parametro.Precision = 18;
            parametro.Scale = 0;
            parametro.Value = valor;
        }

        public bool Eliminar(int idDetallePrestamo, int idPrestamo)
        {
            const string consulta = @"DELETE FROM dbo.Tbl_DetallePrestamo
                WHERE IDDetallePrestamo = @IDDetallePrestamo AND IDPrestamo = @IDPrestamo;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDDetallePrestamo", SqlDbType.Int).Value = idDetallePrestamo;
                    comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = idPrestamo;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo eliminar el detalle del préstamo.", ex);
            }
        }

        public List<DetallePrestamo> ConsultarPorPrestamo(int idPrestamo)
        {
            const string consulta = @"SELECT IDDetallePrestamo, IDPrestamo, Cantidad, Precio, Descuento, Total
                                     FROM dbo.Tbl_DetallePrestamo
                                     WHERE IDPrestamo = @IDPrestamo
                                     ORDER BY IDDetallePrestamo;";
            var detalles = new List<DetallePrestamo>();
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    // El parámetro limita el resultado a las filas del préstamo seleccionado.
                    comando.Parameters.Add("@IDPrestamo", SqlDbType.Int).Value = idPrestamo;
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            detalles.Add(new DetallePrestamo
                            {
                                IDDetallePrestamo = lector.GetInt32(0),
                                IDPrestamo = lector.GetInt32(1),
                                Cantidad = lector.GetInt32(2),
                                Precio = lector.GetDecimal(3),
                                Descuento = lector.IsDBNull(4) ? (decimal?)null : lector.GetDecimal(4),
                                Total = lector.GetDecimal(5)
                            });
                        }
                    }
                }
                return detalles;
            }
            catch (Exception ex) { throw new Exception("No se pudieron consultar los detalles del préstamo.", ex); }
        }
    }
}
