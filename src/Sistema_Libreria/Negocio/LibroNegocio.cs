using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Valida y coordina las operaciones del catalogo de libros.</summary>
    public class LibroNegocio
    {
        private readonly LibroDatos libroDatos = new LibroDatos();

        // Se conserva para las sugerencias de libros que usa el formulario Editoriales.
        public List<Libro> Consultar()
        {
            return libroDatos.Consultar();
        }

        public List<Libro> ConsultarCatalogo()
        {
            return libroDatos.ConsultarCatalogo();
        }

        public List<Libro> Buscar(string criterio)
        {
            return libroDatos.Buscar(criterio ?? string.Empty);
        }

        public int Agregar(Libro libro)
        {
            Validar(libro);
            if (!libroDatos.ExistePedido(libro.IDPedido))
                throw new ArgumentException("El pedido seleccionado no existe.");
            return libroDatos.Agregar(libro);
        }

        public bool Editar(Libro libro)
        {
            Validar(libro);
            if (libro.IDLibro <= 0)
                throw new ArgumentException("Selecciona un libro valido para editar.");
            if (!libroDatos.ExistePedido(libro.IDPedido))
                throw new ArgumentException("El pedido seleccionado no existe.");
            return libroDatos.Editar(libro);
        }

        public bool Eliminar(int idLibro)
        {
            if (idLibro <= 0)
                throw new ArgumentException("Selecciona un libro valido para eliminar.");
            return libroDatos.Eliminar(idLibro);
        }

        private static void Validar(Libro libro)
        {
            if (libro == null)
                throw new ArgumentNullException("libro");
            if (libro.IDPedido <= 0)
                throw new ArgumentException("Selecciona un codigo de pedido valido.");
            if (string.IsNullOrWhiteSpace(libro.Titulo) || libro.Titulo.Trim().Length > 100)
                throw new ArgumentException("El titulo es obligatorio y admite hasta 100 caracteres.");
            if (string.IsNullOrWhiteSpace(libro.ISBN) || libro.ISBN.Trim().Length > 50)
                throw new ArgumentException("El ISBN es obligatorio y admite hasta 50 caracteres.");
            if (libro.Precio < 0 || libro.Precio > 999999999999999999M ||
                decimal.Truncate(libro.Precio) != libro.Precio)
                throw new ArgumentException("El precio debe ser un numero entero positivo con hasta 18 digitos.");

            libro.Titulo = libro.Titulo.Trim();
            libro.ISBN = libro.ISBN.Trim();
        }
    }
}
