using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>
    /// Lee usuarios y el catálogo de sucursales con consultas de solo lectura.
    /// </summary>
    public class UsuariosDatos
    {
        /// <summary>
        /// Verifica el hash y convierte cuentas antiguas después de validar la contraseña.
        /// </summary>
        public Usuario AutenticarUsuario(string nombreUsuario, string contrasena)
        {
            const string consulta = @"
                SELECT TOP (1) IDUsuario, IDCodigoSucursal, NombreUsuario, Estado, Contraseña
                FROM dbo.Tbl_Usuario
                WHERE NombreUsuario = @NombreUsuario
                  AND Estado = 1
                ORDER BY IDUsuario;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = nombreUsuario;
                    conexion.Open();
                    Usuario usuario;
                    string almacenada;
                    using (SqlDataReader lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!lector.Read())
                            return null;

                        almacenada = lector.GetString(4);
                        if (!SeguridadContrasenas.Verificar(contrasena, almacenada)) return null;
                        usuario = new Usuario
                        {
                            IDUsuario = lector.GetInt32(0),
                            IDCodigoSucursal = lector.GetInt32(1),
                            NombreUsuario = lector.GetString(2),
                            Estado = lector.GetBoolean(3)
                        };
                    }
                    if (!SeguridadContrasenas.EsHash(almacenada))
                    {
                        using (var migrar = new SqlCommand(@"UPDATE dbo.Tbl_Usuario SET Contraseña = @Hash
                            WHERE IDUsuario = @ID AND Contraseña = @Anterior;", conexion))
                        {
                            migrar.Parameters.Add("@Hash", SqlDbType.VarChar, 255).Value = SeguridadContrasenas.CrearHash(contrasena);
                            migrar.Parameters.Add("@ID", SqlDbType.Int).Value = usuario.IDUsuario;
                            migrar.Parameters.Add("@Anterior", SqlDbType.VarChar, 255).Value = almacenada;
                            migrar.ExecuteNonQuery();
                        }
                    }
                    return usuario;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al validar el acceso.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo validar el acceso.", ex);
            }
        }

        public int AgregarUsuario(Usuario usuario)
        {
            const string consulta = @"
                INSERT INTO dbo.Tbl_Usuario
                    (IDCodigoSucursal, NombreUsuario, Contraseña, Telefono, Estado)
                VALUES
                    (@IDCodigoSucursal, @NombreUsuario, @Contrasena, @Telefono, @Estado);
                SELECT CONVERT(int, SCOPE_IDENTITY());";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametrosUsuario(comando, usuario);
                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al agregar el usuario.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo agregar el usuario.", ex);
            }
        }

        public bool EditarUsuario(Usuario usuario)
        {
            const string consulta = @"
                UPDATE dbo.Tbl_Usuario
                SET IDCodigoSucursal = @IDCodigoSucursal,
                    NombreUsuario = @NombreUsuario,
                    Contraseña = COALESCE(@Contrasena, Contraseña),
                    Telefono = @Telefono,
                    Estado = @Estado
                WHERE IDUsuario = @IDUsuario;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametrosUsuario(comando, usuario);
                    comando.Parameters.Add("@IDUsuario", SqlDbType.Int).Value = usuario.IDUsuario;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al editar el usuario.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo editar el usuario.", ex);
            }
        }

        public bool EliminarUsuario(int idUsuario)
        {
            const string consulta = @"
                DELETE FROM dbo.Tbl_Usuario
                WHERE IDUsuario = @IDUsuario;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDUsuario", SqlDbType.Int).Value = idUsuario;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al eliminar el usuario.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo eliminar el usuario.", ex);
            }
        }

        public List<Usuario> BuscarUsuarios(string criterio)
        {
            List<Usuario> usuarios = new List<Usuario>();
            const string consulta = @"
                SELECT u.IDUsuario, u.IDCodigoSucursal, s.NombreSucursal,
                       u.NombreUsuario, u.Telefono, u.Estado
                FROM dbo.Tbl_Usuario AS u
                INNER JOIN dbo.Tbl_Sucursales AS s
                    ON s.IDCodigoSucursal = u.IDCodigoSucursal
                WHERE CONVERT(varchar(20), u.IDUsuario) LIKE @Criterio
                   OR CONVERT(varchar(20), u.IDCodigoSucursal) LIKE @Criterio
                   OR u.NombreUsuario LIKE @Criterio
                   OR u.Telefono LIKE @Criterio
                ORDER BY u.IDUsuario;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@Criterio", SqlDbType.VarChar, 8000)
                        .Value = "%" + criterio.Trim() + "%";
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            usuarios.Add(MapearUsuario(lector));
                        }
                    }
                }

                return usuarios;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al buscar usuarios.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron buscar los usuarios.", ex);
            }
        }

        public List<Usuario> ConsultarUsuarios()
        {
            List<Usuario> usuarios = new List<Usuario>();
            string consulta = @"
                SELECT u.IDUsuario, u.IDCodigoSucursal, s.NombreSucursal,
                       u.NombreUsuario, u.Telefono, u.Estado
                FROM dbo.Tbl_Usuario AS u
                INNER JOIN dbo.Tbl_Sucursales AS s
                    ON s.IDCodigoSucursal = u.IDCodigoSucursal
                ORDER BY u.IDUsuario;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            usuarios.Add(MapearUsuario(lector));
                        }
                    }
                }

                return usuarios;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al consultar usuarios.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron consultar los usuarios.", ex);
            }
        }

        public List<Sucursal> ConsultarSucursalesCatalogo()
        {
            List<Sucursal> sucursales = new List<Sucursal>();
            const string consulta = @"
                SELECT IDCodigoSucursal, NombreSucursal
                FROM dbo.Tbl_Sucursales
                ORDER BY NombreSucursal;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            // El catálogo solo necesita el código y el texto visible.
                            sucursales.Add(new Sucursal
                            {
                                IDCodigoSucursal = lector.GetInt32(0),
                                NombreSucursal = lector.GetString(1)
                            });
                        }
                    }
                }

                return sucursales;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al consultar el catálogo de sucursales.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo cargar el catálogo de sucursales.", ex);
            }
        }

        private Usuario MapearUsuario(SqlDataReader lector)
        {
            // Las contraseñas y sus hashes nunca salen hacia los listados de Presentación.
            return new Usuario
            {
                IDUsuario = lector.GetInt32(0),
                IDCodigoSucursal = lector.GetInt32(1),
                NombreSucursal = lector.GetString(2),
                NombreUsuario = lector.GetString(3),
                Telefono = lector.GetString(4),
                Estado = lector.GetBoolean(5)
            };
        }

        private void AgregarParametrosUsuario(SqlCommand comando, Usuario usuario)
        {
            comando.Parameters.Add("@IDCodigoSucursal", SqlDbType.Int).Value = usuario.IDCodigoSucursal;
            comando.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = usuario.NombreUsuario;
            comando.Parameters.Add("@Contrasena", SqlDbType.VarChar, 255).Value =
                string.IsNullOrEmpty(usuario.Contrasena) ? (object)DBNull.Value :
                SeguridadContrasenas.CrearHash(usuario.Contrasena);
            comando.Parameters.Add("@Telefono", SqlDbType.VarChar, 15).Value = usuario.Telefono;
            comando.Parameters.Add("@Estado", SqlDbType.Bit).Value = usuario.Estado;
        }
    }
}
