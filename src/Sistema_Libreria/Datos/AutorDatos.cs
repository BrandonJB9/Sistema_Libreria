using System;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Consultas y cambios parametrizados para el módulo Autor.</summary>
    public class AutorDatos
    {
        public DataTable Consultar(string filtro)
        {
            const string sql = @"SELECT a.IDAutor, a.IDLibro, l.Titulo, a.NombreAutor,
                                        a.Nacionalidad, a.Biografia, a.FechaNacimiento
                                 FROM dbo.Tbl_Autor AS a
                                 INNER JOIN dbo.Tbl_Libros AS l ON l.IDLibro = a.IDLibro
                                 WHERE (@Filtro = ''
                                        OR a.IDAutor = TRY_CONVERT(int, @Filtro)
                                        OR a.IDLibro = TRY_CONVERT(int, @Filtro)
                                        OR a.NombreAutor LIKE '%' + @Filtro + '%'
                                        OR ISNULL(a.Nacionalidad, '') LIKE '%' + @Filtro + '%')
                                 ORDER BY a.IDAutor;";
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
                throw new ApplicationException("No se pudieron consultar los autores. Verifica la conexión con la base de datos. " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ocurrió un problema al preparar la consulta de autores. " + ex.Message, ex);
            }
        }

        /// <summary>Carga el código y el título que se muestran en el selector de libros.</summary>
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

        public void Agregar(AutorEntidad autor)
        {
            const string sql = @"INSERT INTO dbo.Tbl_Autor
                                    (IDLibro, NombreAutor, Nacionalidad, Biografia, FechaNacimiento)
                                 VALUES
                                    (@IDLibro, @NombreAutor, @Nacionalidad, @Biografia, @FechaNacimiento);";
            EjecutarCambio(sql, autor, false);
        }

        public void Editar(AutorEntidad autor)
        {
            const string sql = @"UPDATE dbo.Tbl_Autor
                                 SET IDLibro = @IDLibro,
                                     NombreAutor = @NombreAutor,
                                     Nacionalidad = @Nacionalidad,
                                     Biografia = @Biografia,
                                     FechaNacimiento = @FechaNacimiento
                                 WHERE IDAutor = @IDAutor;";
            EjecutarCambio(sql, autor, true);
        }

        public void Eliminar(int idAutor)
        {
            const string sql = "DELETE FROM dbo.Tbl_Autor WHERE IDAutor = @IDAutor;";
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add("@IDAutor", SqlDbType.Int).Value = idAutor;
                    conexion.Open();
                    if (comando.ExecuteNonQuery() == 0)
                        throw new ApplicationException("No se encontró el autor seleccionado.");
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("No se pudo eliminar el autor. " + ex.Message, ex);
            }
        }

        /// <summary>Obtiene indicadores globales; no dependen de la página visible.</summary>
        public int[] ConsultarTotales()
        {
            const string sql = @"SELECT COUNT(*) AS Total,
                                        SUM(CASE WHEN IDLibro IS NULL THEN 1 ELSE 0 END) AS SinLibro,
                                        SUM(CASE WHEN IDLibro IS NOT NULL THEN 1 ELSE 0 END) AS ConLibro
                                 FROM dbo.Tbl_Autor;";
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        lector.Read();
                        return new int[] { Convert.ToInt32(lector["Total"]),
                            lector["SinLibro"] == DBNull.Value ? 0 : Convert.ToInt32(lector["SinLibro"]),
                            lector["ConLibro"] == DBNull.Value ? 0 : Convert.ToInt32(lector["ConLibro"]) };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("No se pudieron consultar los totales de autores. " + ex.Message, ex);
            }
        }

        private static void EjecutarCambio(string sql, AutorEntidad autor, bool incluirIdAutor)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(ConexionDatos.ObtenerCadenaParaOperaciones()))
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.Add("@IDLibro", SqlDbType.Int).Value = autor.IDLibro;
                    comando.Parameters.Add("@NombreAutor", SqlDbType.VarChar, 100).Value = autor.NombreAutor;
                    comando.Parameters.Add("@Nacionalidad", SqlDbType.VarChar, 50).Value =
                        (object)autor.Nacionalidad ?? DBNull.Value;
                    comando.Parameters.Add("@Biografia", SqlDbType.VarChar, 100).Value = autor.Biografia;
                    comando.Parameters.Add("@FechaNacimiento", SqlDbType.DateTime).Value =
                        (object)autor.FechaNacimiento ?? DBNull.Value;
                    if (incluirIdAutor)
                        comando.Parameters.Add("@IDAutor", SqlDbType.Int).Value = autor.IDAutor;

                    conexion.Open();
                    if (comando.ExecuteNonQuery() == 0 && incluirIdAutor)
                        throw new ApplicationException("No se encontró el autor que se quería editar.");
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("No se pudo guardar el autor. Revisa el libro asociado y los datos ingresados. " + ex.Message, ex);
            }
        }
    }
}
