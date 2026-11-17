using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>
    /// Valida y coordina las operaciones del catalogo de clientes.
    /// </summary>
    public class ClientesNegocio
    {
        private readonly ClientesDatos datos = new ClientesDatos();

        public int AgregarCliente(Cliente cliente)
        {
            ValidarCliente(cliente);
            return datos.AgregarCliente(cliente);
        }

        public void EditarCliente(Cliente cliente)
        {
            ValidarCliente(cliente);
            if (!datos.EditarCliente(cliente))
            {
                throw new InvalidOperationException("No se encontro el cliente seleccionado.");
            }
        }

        public void EliminarCliente(int idCliente)
        {
            if (idCliente <= 0)
            {
                throw new ArgumentException("Selecciona un cliente valido para eliminar.");
            }

            if (!datos.EliminarCliente(idCliente))
            {
                throw new InvalidOperationException("No se encontro el cliente seleccionado.");
            }
        }

        public List<Cliente> BuscarClientes(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
            {
                return datos.ConsultarClientes();
            }

            return datos.BuscarClientes(criterio.Trim());
        }

        public List<Cliente> ConsultarClientes()
        {
            return datos.ConsultarClientes();
        }

        private void ValidarCliente(Cliente cliente)
        {
            if (cliente == null)
            {
                throw new ArgumentNullException("cliente");
            }

            ValidarTexto(cliente.NombreCliente, "Nombre del cliente", 100);
            ValidarTexto(cliente.Telefono, "Telefono", 15);

            if (!string.IsNullOrWhiteSpace(cliente.CorreoElectronico) &&
                cliente.CorreoElectronico.Trim().Length > 50)
            {
                throw new ArgumentException("El correo electronico no puede superar 50 caracteres.");
            }
        }

        private void ValidarTexto(string valor, string campo, int longitudMaxima)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ArgumentException("El campo " + campo + " es obligatorio.");
            }

            if (valor.Trim().Length > longitudMaxima)
            {
                throw new ArgumentException(
                    "El campo " + campo + " no puede superar " + longitudMaxima + " caracteres.");
            }
        }
    }
}
