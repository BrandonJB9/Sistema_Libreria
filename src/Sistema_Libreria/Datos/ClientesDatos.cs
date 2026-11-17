using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>
    /// Ejecuta las consultas parametrizadas del catalogo de clientes.
    /// </summary>
    public class ClientesDatos
    {
        public int AgregarCliente(Cliente cliente)
        {
            const string consulta = @"
                INSERT INTO dbo.Tbl_Cliente (NombreCliente, Telefono, CorreoElectronico)
                VALUES (@NombreCliente, @Telefono, @CorreoElectronico);
                SELECT CONVERT(int, SCOPE_IDENTITY());";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametrosCliente(comando, cliente);
                    conexion.Open();
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al agregar el cliente.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo agregar el cliente.", ex);
            }
        }

        public bool EditarCliente(Cliente cliente)
        {
            const string consulta = @"
                UPDATE dbo.Tbl_Cliente
                SET NombreCliente = @NombreCliente,
                    Telefono = @Telefono,
                    CorreoElectronico = @CorreoElectronico
                WHERE IDCliente = @IDCliente;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    AgregarParametrosCliente(comando, cliente);
                    comando.Parameters.Add("@IDCliente", SqlDbType.Int).Value = cliente.IDCliente;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al editar el cliente.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo editar el cliente.", ex);
            }
        }

        public bool EliminarCliente(int idCliente)
        {
            const string consulta = @"DELETE FROM dbo.Tbl_Cliente WHERE IDCliente = @IDCliente;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@IDCliente", SqlDbType.Int).Value = idCliente;
                    conexion.Open();
                    return comando.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al eliminar el cliente.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo eliminar el cliente.", ex);
            }
        }

        public List<Cliente> BuscarClientes(string criterio)
        {
            const string consulta = @"
                SELECT IDCliente, NombreCliente, Telefono, CorreoElectronico
                FROM dbo.Tbl_Cliente
                WHERE CONVERT(varchar(20), IDCliente) LIKE @Criterio
                   OR NombreCliente LIKE @Criterio
                   OR Telefono LIKE @Criterio
                   OR CorreoElectronico LIKE @Criterio
                ORDER BY IDCliente;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.Add("@Criterio", SqlDbType.VarChar, 8000).Value = "%" + criterio.Trim() + "%";
                    conexion.Open();
                    return LeerClientes(comando);
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al buscar clientes.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron buscar los clientes.", ex);
            }
        }

        public List<Cliente> ConsultarClientes()
        {
            const string consulta = @"
                SELECT IDCliente, NombreCliente, Telefono, CorreoElectronico
                FROM dbo.Tbl_Cliente
                ORDER BY IDCliente;";

            try
            {
                using (SqlConnection conexion = ConexionSqlServer.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    conexion.Open();
                    return LeerClientes(comando);
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Error de SQL al consultar los clientes.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron consultar los clientes.", ex);
            }
        }

        private void AgregarParametrosCliente(SqlCommand comando, Cliente cliente)
        {
            // Los parametros tipados separan los valores de usuario del texto SQL.
            comando.Parameters.Add("@NombreCliente", SqlDbType.VarChar, 100).Value = cliente.NombreCliente;
            comando.Parameters.Add("@Telefono", SqlDbType.VarChar, 15).Value = cliente.Telefono;
            // El correo es opcional en Tbl_Cliente; SQL Server recibe NULL si no se captura.
            comando.Parameters.Add("@CorreoElectronico", SqlDbType.VarChar, 50).Value =
                string.IsNullOrWhiteSpace(cliente.CorreoElectronico)
                    ? (object)DBNull.Value
                    : cliente.CorreoElectronico;
        }

        private List<Cliente> LeerClientes(SqlCommand comando)
        {
            List<Cliente> clientes = new List<Cliente>();
            using (SqlDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    Cliente cliente = new Cliente();
                    cliente.IDCliente = lector.GetInt32(0);
                    cliente.NombreCliente = lector.GetString(1);
                    cliente.Telefono = lector.GetString(2);
                    cliente.CorreoElectronico = lector.IsDBNull(3) ? string.Empty : lector.GetString(3);
                    clientes.Add(cliente);
                }
            }

            return clientes;
        }
    }
}
