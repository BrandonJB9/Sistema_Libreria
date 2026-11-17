using System;
using System.Data;
using Datos;
using Entidad;

namespace Negocio
{
    public class CategoriaNegocio
    {
        private readonly CategoriaDatos datos = new CategoriaDatos();

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

        public void Agregar(CategoriaEntidad categoria)
        {
            ValidarCategoria(categoria);
            datos.Agregar(categoria);
        }

        public void Editar(CategoriaEntidad categoria)
        {
            if (categoria == null)
                throw new ArgumentNullException("categoria", "No se recibieron datos de la categoría.");
            if (categoria.IDCategoria <= 0)
                throw new ArgumentException("Selecciona una categoría válida antes de editar.");
            ValidarCategoria(categoria);
            datos.Editar(categoria);
        }

        public void Eliminar(int idCategoria)
        {
            if (idCategoria <= 0)
                throw new ArgumentException("Selecciona una categoría válida antes de eliminar.");
            datos.Eliminar(idCategoria);
        }

        private static void ValidarCategoria(CategoriaEntidad categoria)
        {
            if (categoria == null)
                throw new ArgumentNullException("categoria", "No se recibieron datos de la categoría.");
            if (categoria.IDLibro <= 0)
                throw new ArgumentException("Selecciona un libro válido.");
            if (string.IsNullOrWhiteSpace(categoria.Categoria))
                throw new ArgumentException("El nombre de la categoría es obligatorio.");
            if (categoria.Categoria.Length > 50)
                throw new ArgumentException("El nombre de la categoría admite hasta 50 caracteres.");
            if (categoria.Descripcion != null && categoria.Descripcion.Length > 100)
                throw new ArgumentException("La descripción admite hasta 100 caracteres.");
        }
    }
}
