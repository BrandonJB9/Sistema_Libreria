using System;

namespace Entidad
{
    /// <summary>Representa un autor y su relación obligatoria con un libro.</summary>
    public class AutorEntidad
    {
        public int IDAutor { get; set; }
        public int IDLibro { get; set; }

        public string NombreAutor { get; set; }
        public string Nacionalidad { get; set; }
        public string Biografia { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }
}
