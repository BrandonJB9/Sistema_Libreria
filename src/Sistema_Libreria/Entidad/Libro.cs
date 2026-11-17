namespace Entidad
{
    /// <summary>Represents a book and its related order.</summary>
    public class Libro
    {
        public int IDLibro { get; set; }
        public int IDPedido { get; set; }
        public string Titulo { get; set; }
        public string ISBN { get; set; }
        public System.DateTime? AnioPublicacion { get; set; }
        public decimal Precio { get; set; }
        public bool? Estado { get; set; }
        public string TipoPedido { get; set; }

        public string Descripcion
        {
            get { return IDLibro + " - " + Titulo + " (ISBN " + ISBN + ")"; }
        }
    }
}
