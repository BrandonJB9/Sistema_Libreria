using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>
    /// Ejecuta consultas de lectura para el catálogo de sucursales.
    /// </summary>
    public class SucursalesDatos
    {
        public int AgregarSucursal(Sucursal sucursal)
        {
            string consulta = @"
                INSERT INTO dbo.Tbl_Sucursales
                    (NombreSucursal, Departamento, Municipio, Direccion,
                     Telefono, HorarioAtencion, Estado)
                VALUES
                    (@NombreSucursal, @Departamento, @Municipio, @Direccion,
                     @Telefono, @HorarioAtencion, @Estado);
                SELECT CONVERT(int, SCOPE_IDENTITY());";

            try
            {
                // IDCodigoSucursal es IDENTITY; SQL Server genera la PK al insertar.
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@NombreSucursal", SqlDbType.VarChar, 100).Value = sucursal.NombreSucursal;
                    comando.Parameters.Add("@Departamento", SqlDbType.VarChar, 50).Value = sucursal.Departamento;
                    comando.Parameters.Add("@Municipio", SqlDbType.VarChar, 50).Value = sucursal.Municipio;
                    comando.Parameters.Add("@Direccion", SqlDbType.VarChar, 100).Value = sucursal.Direccion;
                    comando.Parameters.Add("@Telefono", SqlDbType.VarChar, 15).Value = sucursal.Telefono;
                    comando.Parameters.Add("@HorarioAtencion", SqlDbType.DateTime).Value = sucursal.HorarioAtencion;
                    comando.Parameters.Add("@Estado", SqlDbType.Bit).Value = sucursal.Estado;

                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al agregar la sucursal.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo agregar la sucursal.", ex);
            }
        }

        public bool EditarSucursal(Sucursal sucursal)
        {
            string consulta = @"
                UPDATE dbo.Tbl_Sucursales
                SET NombreSucursal = @NombreSucursal,
                    Departamento = @Departamento,
                    Municipio = @Municipio,
                    Direccion = @Direccion,
                    Telefono = @Telefono,
                    HorarioAtencion = @HorarioAtencion,
                    Estado = @Estado
                WHERE IDCodigoSucursal = @IDCodigoSucursal;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@NombreSucursal", SqlDbType.VarChar, 100).Value = sucursal.NombreSucursal;
                    comando.Parameters.Add("@Departamento", SqlDbType.VarChar, 50).Value = sucursal.Departamento;
                    comando.Parameters.Add("@Municipio", SqlDbType.VarChar, 50).Value = sucursal.Municipio;
                    comando.Parameters.Add("@Direccion", SqlDbType.VarChar, 100).Value = sucursal.Direccion;
                    comando.Parameters.Add("@Telefono", SqlDbType.VarChar, 15).Value = sucursal.Telefono;
                    comando.Parameters.Add("@HorarioAtencion", SqlDbType.DateTime).Value = sucursal.HorarioAtencion;
                    comando.Parameters.Add("@Estado", SqlDbType.Bit).Value = sucursal.Estado;
                    comando.Parameters.Add("@IDCodigoSucursal", SqlDbType.Int).Value = sucursal.IDCodigoSucursal;

                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al editar la sucursal.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo editar la sucursal.", ex);
            }
        }

        public bool EliminarSucursal(int idCodigoSucursal)
        {
            const string consulta = @"
                DELETE FROM dbo.Tbl_Sucursales
                WHERE IDCodigoSucursal = @IDCodigoSucursal;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDCodigoSucursal", SqlDbType.Int).Value = idCodigoSucursal;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al eliminar la sucursal.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo eliminar la sucursal.", ex);
            }
        }

        public List<Sucursal> BuscarSucursales(string criterio)
        {
            List<Sucursal> sucursales = new List<Sucursal>();
            string consulta = @"
                SELECT IDCodigoSucursal, NombreSucursal, Departamento, Municipio,
                       Direccion, Telefono, HorarioAtencion, Estado
                FROM dbo.Tbl_Sucursales
                WHERE CONVERT(varchar(20), IDCodigoSucursal) LIKE @Criterio
                   OR NombreSucursal LIKE @Criterio
                   OR Departamento LIKE @Criterio
                   OR Municipio LIKE @Criterio
                   OR Direccion LIKE @Criterio
                   OR Telefono LIKE @Criterio
                ORDER BY IDCodigoSucursal;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@Criterio", SqlDbType.VarChar, 8000).Value = "%" + criterio.Trim() + "%";
                    conexion.Open();

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            Sucursal sucursal = new Sucursal();
                            sucursal.IDCodigoSucursal = lector.GetInt32(0);
                            sucursal.NombreSucursal = lector.GetString(1);
                            sucursal.Departamento = lector.GetString(2);
                            sucursal.Municipio = lector.GetString(3);
                            sucursal.Direccion = lector.GetString(4);
                            sucursal.Telefono = lector.GetString(5);
                            sucursal.HorarioAtencion = lector.GetDateTime(6);
                            sucursal.Estado = lector.GetBoolean(7);
                            sucursales.Add(sucursal);
                        }
                    }
                }

                return sucursales;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al buscar sucursales.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron buscar las sucursales.", ex);
            }
        }

        public List<Sucursal> ConsultarSucursales()
        {
            List<Sucursal> sucursales = new List<Sucursal>();
            string consulta = @"
                SELECT IDCodigoSucursal, NombreSucursal, Departamento, Municipio,
                       Direccion, Telefono, HorarioAtencion, Estado
                FROM dbo.Tbl_Sucursales
                ORDER BY IDCodigoSucursal;";

            try
            {
                // Se reutiliza la conexión configurada en el Documento 01.
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();

                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            Sucursal sucursal = new Sucursal();
                            sucursal.IDCodigoSucursal = lector.GetInt32(0);
                            sucursal.NombreSucursal = lector.GetString(1);
                            sucursal.Departamento = lector.GetString(2);
                            sucursal.Municipio = lector.GetString(3);
                            sucursal.Direccion = lector.GetString(4);
                            sucursal.Telefono = lector.GetString(5);
                            sucursal.HorarioAtencion = lector.GetDateTime(6);
                            sucursal.Estado = lector.GetBoolean(7);
                            sucursales.Add(sucursal);
                        }
                    }
                }

                return sucursales;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al consultar las sucursales.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron consultar las sucursales.", ex);
            }
        }
    }
}
