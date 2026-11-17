using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Entrega a Presentacion los pedidos para seleccionar la relacion.</summary>
    public class PedidoNegocio
    {
        private readonly PedidoDatos pedidoDatos = new PedidoDatos();

        public List<Pedido> Consultar()
        {
            return pedidoDatos.Consultar();
        }
    }
}
