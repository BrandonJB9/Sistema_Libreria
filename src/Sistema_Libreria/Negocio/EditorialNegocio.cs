using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Reglas sencillas para validar y administrar editoriales.</summary>
    public class EditorialNegocio
    {
        private readonly EditorialDatos editorialDatos = new EditorialDatos();

        public List<Editorial> Consultar()
        {
            return editorialDatos.Consultar();
        }

        public List<Editorial> Buscar(string criterio)
        {
            return editorialDatos.Buscar(criterio ?? string.Empty);
        }

        public int Agregar(Editorial editorial)
        {
            Validar(editorial);
            if (!editorialDatos.ExisteLibro(editorial.IDLibro))
                throw new ArgumentException("El código de libro indicado no existe.");
            return editorialDatos.Agregar(editorial);
        }

        public bool Editar(Editorial editorial)
        {
            Validar(editorial);
            if (editorial.IDEditorial <= 0)
                throw new ArgumentException("Selecciona una editorial válida para editar.");
            if (!editorialDatos.ExisteLibro(editorial.IDLibro))
                throw new ArgumentException("El código de libro indicado no existe.");
            return editorialDatos.Editar(editorial);
        }

        public bool Eliminar(int idEditorial)
        {
            if (idEditorial <= 0)
                throw new ArgumentException("Selecciona una editorial válida para eliminar.");
            return editorialDatos.Eliminar(idEditorial);
        }

        private static void Validar(Editorial editorial)
        {
            if (editorial == null)
                throw new ArgumentNullException("editorial");
            if (editorial.IDLibro <= 0)
                throw new ArgumentException("Escribe un código de libro válido.");
            if (string.IsNullOrWhiteSpace(editorial.NombreEditorial) || editorial.NombreEditorial.Trim().Length > 100)
                throw new ArgumentException("El nombre de la editorial es obligatorio y admite hasta 100 caracteres.");
            if (string.IsNullOrWhiteSpace(editorial.Pais) || editorial.Pais.Trim().Length > 50)
                throw new ArgumentException("El país es obligatorio y admite hasta 50 caracteres.");
            if (editorial.Direccion != null && editorial.Direccion.Trim().Length > 100)
                throw new ArgumentException("La dirección admite hasta 100 caracteres.");

            editorial.NombreEditorial = editorial.NombreEditorial.Trim();
            editorial.Pais = editorial.Pais.Trim();
            editorial.Direccion = string.IsNullOrWhiteSpace(editorial.Direccion) ? null : editorial.Direccion.Trim();
        }
    }
}
