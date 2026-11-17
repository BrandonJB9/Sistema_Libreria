using System;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    public class CategoriaDatos
    {
        public DataTable Consultar(string filtro)
        {
            const string sql = @"SELECT c.IDCategoria, c.IDLibro, l.Titulo,
                                        c.Categoria, c.Descripcion, c.Estado
                                 FROM dbo.Tbl_Categoria AS c
                                 INNER JOIN dbo.Tbl_Libros AS l ON l.IDLibro = c.IDLibro
                                 WHERE (@Filtro = ''
                                        OR c.IDCategoria = TRY_CONVERT(int, @Filtro)
                                        OR c.IDLibro = TRY_CONVERT(int, @Filtro)
                                        OR c.Categoria LIKE '%' + @Filtro + '%'
                                        OR ISNULL(c.Descripcion, '') LIKE '%' + @Filtro + '%')
                                 ORDER BY c.IDCategoria;";
            DataTable tabla = new DataTable();
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                using (SqlDataAdapter adaptador = new SqlDataAdapter(comando))
                {
                    comando.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = filtro ?? string.Empty;
                    adaptador.Fill(tabla);
                }
                return tabla;
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("No se pudieron consultar las categorías. Verifica la conexión con la base de datos. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ocurrió un problema al preparar la consulta de categorías. " + ex.Message, ex);
            }
        }

        public DataTable ConsultarCatalogoLibros()
        {
            const string sql = "SELECT IDLibro, Titulo FROM dbo.Tbl_Libros ORDER BY Titulo;";
            DataTable tabla = new DataTable();
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (SqlDataAdapter adaptador = new SqlDataAdapter(sql, conexion))
                {
                    adaptador.Fill(tabla);
                }
                return tabla;
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("No se pudo cargar el catálogo de libros. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ocurrió un problema al preparar el catálogo de libros. " + ex.Message, ex);
            }
        }

        public void Agregar(CategoriaEntidad categoria)
        {
            const string sql = @"INSERT INTO dbo.Tbl_Categoria (IDLibro, Categoria, Descripcion, Estado)
                                 VALUES (@IDLibro, @Categoria, @Descripcion, @Estado);";
            EjecutarCambio(sql, categoria, false);
        }

        public void Editar(CategoriaEntidad categoria)
        {
            const string sql = @"UPDATE dbo.Tbl_Categoria
                                 SET IDLibro = @IDLibro,
                                     Categoria = @Categoria,
                                     Descripcion = @Descripcion,
                                     Estado = @Estado
                                 WHERE IDCategoria = @IDCategoria;";
            EjecutarCambio(sql, categoria, true);
        }

        public void Eliminar(int idCategoria)
        {
            const string sql = "DELETE FROM dbo.Tbl_Categoria WHERE IDCategoria = @IDCategoria;";
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add("@IDCategoria", SqlDbType.Int).Value = idCategoria;
                    conexion.Open();
                    if (comando.ExecuteNonQuery() == 0)
                        throw new ApplicationException("No se encontró la categoría seleccionada.");
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("No se pudo eliminar la categoría. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ocurrió un problema al eliminar la categoría. " + ex.Message, ex);
            }
        }

        public int[] ConsultarTotales()
        {
            const string sql = @"SELECT COUNT(*) AS Total,
                                        SUM(CASE WHEN Estado = 1 THEN 1 ELSE 0 END) AS Disponibles,
                                        SUM(CASE WHEN Estado = 0 THEN 1 ELSE 0 END) AS NoDisponibles
                                 FROM dbo.Tbl_Categoria;";
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        lector.Read();
                        return new int[]
                        {
                            Convert.ToInt32(lector["Total"]),
                            lector["Disponibles"] == DBNull.Value ? 0 : Convert.ToInt32(lector["Disponibles"]),
                            lector["NoDisponibles"] == DBNull.Value ? 0 : Convert.ToInt32(lector["NoDisponibles"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("No se pudieron consultar los totales de categorías. " + ex.Message, ex);
            }
        }

        private static void EjecutarCambio(string sql, CategoriaEntidad categoria, bool incluirIdCategoria)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add("@IDLibro", SqlDbType.Int).Value = categoria.IDLibro;
                    comando.Parameters.Add("@Categoria", SqlDbType.VarChar, 50).Value = categoria.Categoria;
                    comando.Parameters.Add("@Descripcion", SqlDbType.VarChar, 100).Value =
                        (object)categoria.Descripcion ?? DBNull.Value;
                    comando.Parameters.Add("@Estado", SqlDbType.Bit).Value = categoria.Estado;
                    if (incluirIdCategoria)
                        comando.Parameters.Add("@IDCategoria", SqlDbType.Int).Value = categoria.IDCategoria;

                    conexion.Open();
                    if (comando.ExecuteNonQuery() == 0 && incluirIdCategoria)
                        throw new ApplicationException("No se encontró la categoría que se quería editar.");
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("No se pudo guardar la categoría. Revisa el libro asociado y los datos ingresados. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ocurrió un problema al guardar la categoría. " + ex.Message, ex);
            }
        }
    }
}
