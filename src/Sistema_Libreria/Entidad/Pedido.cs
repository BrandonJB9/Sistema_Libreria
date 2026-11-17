namespace Entidad
{
    /// <summary>Representa un pedido de la biblioteca.</summary>
    public class Pedido
    {
        public int IDPedido { get; set; }
        public int IDCodigoSucursal { get; set; }
        public int IDCliente { get; set; }
        public string TipoPedido { get; set; }
        public System.DateTime FechaPedido { get; set; }
        public bool Estado { get; set; }
        public string Descripcion { get { return IDPedido + " - " + TipoPedido; } }
    }
}
