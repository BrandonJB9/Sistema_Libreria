namespace Entidad
{
    /// <summary>Representa una editorial registrada y el libro asociado.</summary>
    public class Editorial
    {
        public int IDEditorial { get; set; }
        public int IDLibro { get; set; }
        public string NombreEditorial { get; set; }
        public string Pais { get; set; }
        public string Direccion { get; set; }
        public bool Estado { get; set; }
        public string TituloLibro { get; set; }
    }
}
