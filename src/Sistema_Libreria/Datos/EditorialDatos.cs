using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Acceso ADO.NET parametrizado para editoriales.</summary>
    public class EditorialDatos
    {
        public List<Editorial> Consultar()
        {
            const string consulta = @"SELECT e.IDEditorial, e.IDLibro, e.NombreEditorial, e.Pais,
                                            e.Direccion, e.Estado, l.Titulo AS TituloLibro
                                     FROM dbo.Tbl_Editorial AS e
                                     INNER JOIN dbo.Tbl_Libros AS l ON l.IDLibro = e.IDLibro
                                     ORDER BY e.IDEditorial;";
            var lista = new List<Editorial>();
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                            lista.Add(Mapear(lector));
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron consultar las editoriales.", ex);
            }
        }

        public List<Editorial> Buscar(string criterio)
        {
            const string consulta = @"SELECT e.IDEditorial, e.IDLibro, e.NombreEditorial, e.Pais,
                                            e.Direccion, e.Estado, l.Titulo AS TituloLibro
                                     FROM dbo.Tbl_Editorial AS e
                                     INNER JOIN dbo.Tbl_Libros AS l ON l.IDLibro = e.IDLibro
                                     WHERE (@Codigo IS NOT NULL AND e.IDEditorial = @Codigo)
                                        OR e.NombreEditorial LIKE @Texto
                                        OR l.Titulo LIKE @Texto
                                     ORDER BY e.IDEditorial;";
            var lista = new List<Editorial>();
            int codigo;
            bool esCodigo = int.TryParse(criterio, out codigo);
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@Codigo", SqlDbType.Int).Value = esCodigo ? (object)codigo : DBNull.Value;
                    comando.Parameters.Add("@Texto", SqlDbType.VarChar, 102).Value = "%" + criterio.Trim() + "%";
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                            lista.Add(Mapear(lector));
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron buscar las editoriales.", ex);
            }
        }

        public int Agregar(Editorial editorial)
        {
            const string consulta = @"INSERT INTO dbo.Tbl_Editorial
                                         (IDLibro, NombreEditorial, Pais, Direccion, Estado)
                                      OUTPUT INSERTED.IDEditorial
                                      VALUES (@IDLibro, @NombreEditorial, @Pais, @Direccion, @Estado);";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametros(comando, editorial);
                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo agregar la editorial. Verifica que el código de libro exista.", ex);
            }
        }

        public bool Editar(Editorial editorial)
        {
            const string consulta = @"UPDATE dbo.Tbl_Editorial
                                      SET IDLibro = @IDLibro, NombreEditorial = @NombreEditorial,
                                          Pais = @Pais, Direccion = @Direccion, Estado = @Estado
                                      WHERE IDEditorial = @IDEditorial;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametros(comando, editorial);
                    comando.Parameters.Add("@IDEditorial", SqlDbType.Int).Value = editorial.IDEditorial;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo editar la editorial.", ex);
            }
        }

        public bool Eliminar(int idEditorial)
        {
            const string consulta = "DELETE FROM dbo.Tbl_Editorial WHERE IDEditorial = @IDEditorial;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDEditorial", SqlDbType.Int).Value = idEditorial;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo eliminar la editorial.", ex);
            }
        }

        public bool ExisteLibro(int idLibro)
        {
            const string consulta = "SELECT COUNT(1) FROM dbo.Tbl_Libros WHERE IDLibro = @IDLibro;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDLibro", SqlDbType.Int).Value = idLibro;
                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo verificar el libro relacionado.", ex);
            }
        }

        private static void AgregarParametros(SqlCommand comando, Editorial editorial)
        {
            comando.Parameters.Add("@IDLibro", SqlDbType.Int).Value = editorial.IDLibro;
            comando.Parameters.Add("@NombreEditorial", SqlDbType.VarChar, 100).Value = editorial.NombreEditorial;
            comando.Parameters.Add("@Pais", SqlDbType.VarChar, 50).Value = editorial.Pais;
            comando.Parameters.Add("@Direccion", SqlDbType.VarChar, 100).Value =
                string.IsNullOrWhiteSpace(editorial.Direccion) ? (object)DBNull.Value : editorial.Direccion;
            comando.Parameters.Add("@Estado", SqlDbType.Bit).Value = editorial.Estado;
        }

        private static Editorial Mapear(SqlDataReader lector)
        {
            return new Editorial
            {
                IDEditorial = lector.GetInt32(lector.GetOrdinal("IDEditorial")),
                IDLibro = lector.GetInt32(lector.GetOrdinal("IDLibro")),
                NombreEditorial = lector.GetString(lector.GetOrdinal("NombreEditorial")),
                Pais = lector.GetString(lector.GetOrdinal("Pais")),
                Direccion = lector.IsDBNull(lector.GetOrdinal("Direccion")) ? string.Empty : lector.GetString(lector.GetOrdinal("Direccion")),
                Estado = lector.GetBoolean(lector.GetOrdinal("Estado")),
                TituloLibro = lector.GetString(lector.GetOrdinal("TituloLibro"))
            };
        }
    }
}
