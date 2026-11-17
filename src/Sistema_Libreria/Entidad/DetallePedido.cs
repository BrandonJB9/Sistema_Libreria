namespace Entidad
{
    /// <summary>Representa una línea de dbo.Tbl_DetallePedido.</summary>
    public class DetallePedido
    {
        public int IDDetallePedido { get; set; }
        public int IDPedido { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string Observaciones { get; set; }
        public decimal Total { get; set; }
    }
}
