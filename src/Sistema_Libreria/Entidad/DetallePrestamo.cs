namespace Entidad
{
    /// <summary>Datos de consulta de una fila de detalle de préstamo.</summary>
    public class DetallePrestamo
    {
        public int IDDetallePrestamo { get; set; }
        public int IDPrestamo { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal? Descuento { get; set; }
        public decimal Total { get; set; }
    }
}
