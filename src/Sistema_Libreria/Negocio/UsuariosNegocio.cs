using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>
    /// Expone consultas sencillas de usuarios y sucursales a Presentación.
    /// </summary>
    public class UsuariosNegocio
    {
        private readonly UsuariosDatos datos = new UsuariosDatos();

        public Usuario AutenticarUsuario(string nombreUsuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("Escribe el nombre de usuario.");
            if (string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("Escribe la contraseña.");
            if (nombreUsuario.Trim().Length > 50)
                throw new ArgumentException("El nombre de usuario no puede superar 50 caracteres.");
            if (contrasena.Length > 50)
                throw new ArgumentException("La contraseña no puede superar 50 caracteres.");

            try
            {
                return datos.AutenticarUsuario(nombreUsuario.Trim(), contrasena);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible validar el inicio de sesión.", ex);
            }
        }

        public int AgregarUsuario(Usuario usuario)
        {
            try
            {
                ValidarUsuario(usuario);
                if (string.IsNullOrWhiteSpace(usuario.Contrasena))
                    throw new ArgumentException("La contraseña es obligatoria para un usuario nuevo.");
                return datos.AgregarUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible agregar el usuario. " + ex.Message, ex);
            }
        }

        public void EditarUsuario(Usuario usuario)
        {
            try
            {
                ValidarUsuario(usuario);
                if (usuario.IDUsuario <= 0)
                    throw new ArgumentException("Selecciona un usuario válido para editar.");
                if (!datos.EditarUsuario(usuario))
                    throw new InvalidOperationException("No se encontró el usuario seleccionado.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible editar el usuario. " + ex.Message, ex);
            }
        }

        public void EliminarUsuario(int idUsuario)
        {
            try
            {
                if (idUsuario <= 0)
                    throw new ArgumentException("Selecciona un usuario válido para eliminar.");
                if (!datos.EliminarUsuario(idUsuario))
                    throw new InvalidOperationException("No se encontró el usuario seleccionado.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible eliminar el usuario. " + ex.Message, ex);
            }
        }

        public List<Usuario> BuscarUsuarios(string criterio)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(criterio))
                    return datos.ConsultarUsuarios();
                return datos.BuscarUsuarios(criterio.Trim());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible buscar usuarios.", ex);
            }
        }

        public List<Usuario> ConsultarUsuarios()
        {
            try
            {
                return datos.ConsultarUsuarios();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible obtener la lista de usuarios.", ex);
            }
        }

        public List<Sucursal> ConsultarSucursalesCatalogo()
        {
            try
            {
                return datos.ConsultarSucursalesCatalogo();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible obtener el catálogo de sucursales.", ex);
            }
        }

        private void ValidarUsuario(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException("usuario");
            if (usuario.IDCodigoSucursal <= 0)
                throw new ArgumentException("Selecciona una sucursal válida.");
            ValidarTexto(usuario.NombreUsuario, "Nombre de usuario", 50);
            // Al editar, un campo vacío conserva la contraseña existente.
            if (!string.IsNullOrEmpty(usuario.Contrasena) &&
                (string.IsNullOrWhiteSpace(usuario.Contrasena) || usuario.Contrasena.Length > 50))
                throw new ArgumentException("La contraseña debe contener entre 1 y 50 caracteres y no solo espacios.");
            ValidarTexto(usuario.Telefono, "Teléfono", 15);
        }

        private void ValidarTexto(string valor, string campo, int maximo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El campo " + campo + " es obligatorio.");
            if (valor.Trim().Length > maximo)
                throw new ArgumentException("El campo " + campo + " no puede superar " + maximo + " caracteres.");
        }
    }
}
