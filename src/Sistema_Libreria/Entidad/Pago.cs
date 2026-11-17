namespace Entidad
{
    /// <summary>Representa un pago y sus relaciones obligatorias.</summary>
    public class Pago
    {
        public int IDagos { get; set; }
        public string TipoPago { get; set; }
        public string TipoServicio { get; set; }
        public string Comprobante { get; set; }
        public int IDPedido { get; set; }
        public int IDPrestamo { get; set; }
        public int IDDevoluciones { get; set; }

        // Descripciones para mostrar las relaciones en el listado.
        public string TipoPedido { get; set; }
        public System.DateTime FechaPrestamo { get; set; }
        public System.DateTime FechaDevolucion { get; set; }
    }
}
