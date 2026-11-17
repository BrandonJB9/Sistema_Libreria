using System.Data;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Métodos sencillos de consulta para el módulo Autor.</summary>
    public class AutorNegocio
    {
        private readonly AutorDatos datos = new AutorDatos();

        public DataTable Consultar(string filtro)
        {
            return datos.Consultar(filtro);
        }

        public DataTable ConsultarCatalogoLibros()
        {
            return datos.ConsultarCatalogoLibros();
        }

        public int[] ConsultarTotales()
        {
            return datos.ConsultarTotales();
        }

        public void Agregar(AutorEntidad autor)
        {
            ValidarAutor(autor);
            datos.Agregar(autor);
        }

        public void Editar(AutorEntidad autor)
        {
            ValidarAutor(autor);
            if (autor.IDAutor <= 0)
                throw new System.ArgumentException("Selecciona un autor válido antes de editar.");
            datos.Editar(autor);
        }

        public void Eliminar(int idAutor)
        {
            if (idAutor <= 0)
                throw new System.ArgumentException("Selecciona un autor válido antes de eliminar.");
            datos.Eliminar(idAutor);
        }

        private static void ValidarAutor(AutorEntidad autor)
        {
            if (autor == null)
                throw new System.ArgumentNullException("autor", "No se recibieron datos del autor.");
            if (autor.IDLibro <= 0)
                throw new System.ArgumentException("Selecciona un libro válido.");
            if (string.IsNullOrWhiteSpace(autor.NombreAutor))
                throw new System.ArgumentException("El nombre del autor es obligatorio.");
            if (autor.NombreAutor.Length > 100)
                throw new System.ArgumentException("El nombre admite hasta 100 caracteres.");
            if (autor.Nacionalidad != null && autor.Nacionalidad.Length > 50)
                throw new System.ArgumentException("La nacionalidad admite hasta 50 caracteres.");
            if (string.IsNullOrWhiteSpace(autor.Biografia))
                throw new System.ArgumentException("La biografía es obligatoria.");
            if (autor.Biografia.Length > 100)
                throw new System.ArgumentException("La biografía admite hasta 100 caracteres.");
        }
    }
}
