using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Acceso a datos para el catalogo de libros.</summary>
    public class LibroDatos
    {
        // Esta consulta sencilla tambien se usa para sugerencias en Editoriales.
        public List<Libro> Consultar()
        {
            const string consulta = "SELECT IDLibro, Titulo, ISBN FROM dbo.Tbl_Libros ORDER BY Titulo;";
            var lista = new List<Libro>();
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                            lista.Add(new Libro
                            {
                                IDLibro = lector.GetInt32(0),
                                Titulo = lector.GetString(1),
                                ISBN = lector.GetString(2)
                            });
                    }
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron consultar los libros.", ex);
            }
        }

        public List<Libro> ConsultarCatalogo()
        {
            const string consulta = @"SELECT l.IDLibro, l.IDPedido, l.Titulo, l.ISBN,
                                            l.[AñoPublicacion] AS AnioPublicacion, l.Precio,
                                            l.Estado, p.TipoPedido
                                     FROM dbo.Tbl_Libros AS l
                                     INNER JOIN dbo.Tbl_Pedidos AS p ON p.IDPedido = l.IDPedido
                                     ORDER BY l.IDLibro;";
            return EjecutarConsulta(consulta, null, null);
        }

        public List<Libro> Buscar(string criterio)
        {
            const string consulta = @"SELECT l.IDLibro, l.IDPedido, l.Titulo, l.ISBN,
                                            l.[AñoPublicacion] AS AnioPublicacion, l.Precio,
                                            l.Estado, p.TipoPedido
                                     FROM dbo.Tbl_Libros AS l
                                     INNER JOIN dbo.Tbl_Pedidos AS p ON p.IDPedido = l.IDPedido
                                     WHERE (@Codigo IS NOT NULL AND l.IDLibro = @Codigo)
                                        OR l.Titulo LIKE @Texto
                                        OR l.ISBN LIKE @Texto
                                        OR p.TipoPedido LIKE @Texto
                                     ORDER BY l.IDLibro;";
            int codigo;
            bool esCodigo = int.TryParse(criterio, out codigo);
            return EjecutarConsulta(consulta, esCodigo ? (object)codigo : DBNull.Value, "%" + criterio.Trim() + "%");
        }

        public int Agregar(Libro libro)
        {
            const string consulta = @"INSERT INTO dbo.Tbl_Libros
                                         (IDPedido, Titulo, ISBN, [AñoPublicacion], Precio, Estado)
                                      OUTPUT INSERTED.IDLibro
                                      VALUES (@IDPedido, @Titulo, @ISBN, @AnioPublicacion, @Precio, @Estado);";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametros(comando, libro);
                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo agregar el libro.", ex);
            }
        }

        public bool Editar(Libro libro)
        {
            const string consulta = @"UPDATE dbo.Tbl_Libros
                                      SET IDPedido = @IDPedido, Titulo = @Titulo, ISBN = @ISBN,
                                          [AñoPublicacion] = @AnioPublicacion, Precio = @Precio, Estado = @Estado
                                      WHERE IDLibro = @IDLibro;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametros(comando, libro);
                    comando.Parameters.Add("@IDLibro", SqlDbType.Int).Value = libro.IDLibro;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo actualizar el libro.", ex);
            }
        }

        public bool Eliminar(int idLibro)
        {
            const string consulta = "DELETE FROM dbo.Tbl_Libros WHERE IDLibro = @IDLibro;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDLibro", SqlDbType.Int).Value = idLibro;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                    throw new InvalidOperationException(
                        "No se puede eliminar el libro porque tiene autores, categorias o editoriales relacionados.", ex);
                throw new Exception("No se pudo eliminar el libro.", ex);
            }
        }

        public bool ExistePedido(int idPedido)
        {
            const string consulta = "SELECT COUNT(1) FROM dbo.Tbl_Pedidos WHERE IDPedido = @IDPedido;";
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = idPedido;
                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo validar el pedido relacionado.", ex);
            }
        }

        private static List<Libro> EjecutarConsulta(string consulta, object codigo, object texto)
        {
            var lista = new List<Libro>();
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (var comando = new SqlCommand(consulta, conexion))
                {
                    if (codigo != null)
                    {
                        comando.Parameters.Add("@Codigo", SqlDbType.Int).Value = codigo;
                        comando.Parameters.Add("@Texto", SqlDbType.VarChar, 102).Value = texto;
                    }

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
                throw new Exception("No se pudo consultar el catalogo de libros.", ex);
            }
        }

        private static void AgregarParametros(SqlCommand comando, Libro libro)
        {
            comando.Parameters.Add("@IDPedido", SqlDbType.Int).Value = libro.IDPedido;
            comando.Parameters.Add("@Titulo", SqlDbType.VarChar, 100).Value = libro.Titulo;
            comando.Parameters.Add("@ISBN", SqlDbType.VarChar, 50).Value = libro.ISBN;

            SqlParameter fecha = comando.Parameters.Add("@AnioPublicacion", SqlDbType.DateTime);
            fecha.Value = libro.AnioPublicacion.HasValue ? (object)libro.AnioPublicacion.Value : DBNull.Value;

            SqlParameter precio = comando.Parameters.Add("@Precio", SqlDbType.Decimal);
            precio.Precision = 18;
            precio.Scale = 0;
            precio.Value = libro.Precio;

            comando.Parameters.Add("@Estado", SqlDbType.Bit).Value =
                libro.Estado.HasValue ? (object)libro.Estado.Value : DBNull.Value;
        }

        private static Libro Mapear(SqlDataReader lector)
        {
            int ordinalFecha = lector.GetOrdinal("AnioPublicacion");
            int ordinalEstado = lector.GetOrdinal("Estado");
            return new Libro
            {
                IDLibro = lector.GetInt32(lector.GetOrdinal("IDLibro")),
                IDPedido = lector.GetInt32(lector.GetOrdinal("IDPedido")),
                Titulo = lector.GetString(lector.GetOrdinal("Titulo")),
                ISBN = lector.GetString(lector.GetOrdinal("ISBN")),
                AnioPublicacion = lector.IsDBNull(ordinalFecha) ? (DateTime?)null : lector.GetDateTime(ordinalFecha),
                Precio = lector.GetDecimal(lector.GetOrdinal("Precio")),
                Estado = lector.IsDBNull(ordinalEstado) ? (bool?)null : lector.GetBoolean(ordinalEstado),
                TipoPedido = lector.GetString(lector.GetOrdinal("TipoPedido"))
            };
        }
    }
}
