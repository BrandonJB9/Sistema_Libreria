using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Coordina las consultas de solo lectura para pedidos.</summary>
    public class PedidosNegocio
    {
        private readonly PedidosDatos datos = new PedidosDatos();

        public int AgregarPedido(Pedido pedido)
        {
            if (pedido == null)
                throw new ArgumentNullException("pedido");
            if (pedido.IDCodigoSucursal <= 0)
                throw new ArgumentException("Escribe un código de sucursal válido.");
            if (pedido.IDCliente <= 0)
                throw new ArgumentException("Escribe un ID de cliente válido.");
            if (string.IsNullOrWhiteSpace(pedido.TipoPedido) || pedido.TipoPedido.Trim().Length > 100)
                throw new ArgumentException("Selecciona un tipo de pedido válido.");
            pedido.TipoPedido = pedido.TipoPedido.Trim();
            try
            {
                return datos.AgregarPedido(pedido);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible guardar el encabezado del pedido.", ex);
            }
        }

        public int AgregarDetallePedido(DetallePedido detalle)
        {
            if (detalle == null)
                throw new ArgumentNullException("detalle");
            if (detalle.IDPedido <= 0)
                throw new ArgumentException("Primero guarda o selecciona un encabezado de pedido.");
            if (detalle.Cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero.");

            decimal valorMaximo = 999999999999999999m;
            if (detalle.Precio < 0 || detalle.Precio > valorMaximo)
                throw new ArgumentException("El precio debe estar dentro del rango permitido.");
            if (!string.IsNullOrEmpty(detalle.Observaciones) && detalle.Observaciones.Length > 100)
                throw new ArgumentException("Las observaciones no pueden superar 100 caracteres.");

            detalle.Total = detalle.Cantidad * detalle.Precio;
            if (detalle.Total > valorMaximo)
                throw new ArgumentException("El total de la línea supera el rango permitido por la tabla.");

            try
            {
                return datos.AgregarDetallePedido(detalle);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible guardar el detalle del pedido.", ex);
            }
        }

        public List<Pedido> ConsultarPedidos()
        {
            try
            {
                return datos.ConsultarPedidos();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible cargar los encabezados de pedido.", ex);
            }
        }

        public List<Pedido> BuscarPedidos(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio)) return ConsultarPedidos();
            try { return datos.BuscarPedidos(criterio.Trim()); }
            catch (Exception ex) { throw new InvalidOperationException("No fue posible buscar los pedidos.", ex); }
        }

        public bool EditarPedido(Pedido pedido)
        {
            ValidarPedido(pedido);
            if (pedido.IDPedido <= 0) throw new ArgumentException("Selecciona un pedido válido.");
            try { if (!datos.EditarPedido(pedido)) throw new InvalidOperationException("El pedido seleccionado ya no existe."); return true; }
            catch (Exception ex) { throw new InvalidOperationException("No fue posible editar el pedido.", ex); }
        }

        private void ValidarPedido(Pedido pedido)
        {
            if (pedido == null) throw new ArgumentNullException("pedido");
            if (pedido.IDCodigoSucursal <= 0) throw new ArgumentException("Selecciona una sucursal válida.");
            if (pedido.IDCliente <= 0) throw new ArgumentException("Selecciona un cliente válido.");
            if (string.IsNullOrWhiteSpace(pedido.TipoPedido) || pedido.TipoPedido.Trim().Length > 100)
                throw new ArgumentException("Selecciona un tipo de pedido válido.");
            pedido.TipoPedido = pedido.TipoPedido.Trim();
        }

        public bool EliminarPedido(int idPedido)
        {
            if (idPedido <= 0) throw new ArgumentException("Selecciona un pedido válido.");
            try { if (!datos.EliminarPedido(idPedido)) throw new InvalidOperationException("El pedido seleccionado ya no existe."); return true; }
            catch (Exception ex) { throw new InvalidOperationException("No fue posible eliminar el pedido.", ex); }
        }

        public List<DetallePedido> ConsultarDetallesPorPedido(int idPedido)
        {
            if (idPedido <= 0)
                throw new ArgumentException("Selecciona un encabezado de pedido válido.", "idPedido");

            try
            {
                return datos.ConsultarDetallesPorPedido(idPedido);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible cargar los detalles del pedido.", ex);
            }
        }

        public List<DetallePedido> BuscarDetallesPorPedido(int idPedido, string criterio)
        {
            if (idPedido <= 0) throw new ArgumentException("Selecciona un pedido válido.", "idPedido");
            if (string.IsNullOrWhiteSpace(criterio)) return ConsultarDetallesPorPedido(idPedido);
            try { return datos.BuscarDetallesPorPedido(idPedido, criterio.Trim()); }
            catch (Exception ex) { throw new InvalidOperationException("No fue posible buscar los detalles del pedido.", ex); }
        }

        public bool EditarDetallePedido(DetallePedido detalle)
        {
            ValidarDetalle(detalle);
            if (detalle.IDDetallePedido <= 0) throw new ArgumentException("Selecciona un detalle válido.");
            try { if (!datos.EditarDetallePedido(detalle)) throw new InvalidOperationException("El detalle seleccionado ya no existe en este pedido."); return true; }
            catch (Exception ex) { throw new InvalidOperationException("No fue posible editar el detalle.", ex); }
        }

        public bool EliminarDetallePedido(int idPedido, int idDetalle)
        {
            if (idPedido <= 0 || idDetalle <= 0) throw new ArgumentException("Selecciona un detalle válido.");
            try { if (!datos.EliminarDetallePedido(idPedido, idDetalle)) throw new InvalidOperationException("El detalle seleccionado ya no existe en este pedido."); return true; }
            catch (Exception ex) { throw new InvalidOperationException("No fue posible eliminar el detalle.", ex); }
        }

        private void ValidarDetalle(DetallePedido detalle)
        {
            if (detalle == null) throw new ArgumentNullException("detalle");
            if (detalle.IDPedido <= 0) throw new ArgumentException("Selecciona un pedido válido.");
            if (detalle.Cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que cero.");
            decimal valorMaximo = 999999999999999999m;
            if (detalle.Precio < 0 || detalle.Precio > valorMaximo) throw new ArgumentException("El precio está fuera del rango permitido.");
            if (!string.IsNullOrEmpty(detalle.Observaciones) && detalle.Observaciones.Length > 100) throw new ArgumentException("Las observaciones no pueden superar 100 caracteres.");
            detalle.Total = detalle.Cantidad * detalle.Precio;
            if (detalle.Total > valorMaximo) throw new ArgumentException("El total supera el rango permitido.");
        }
    }
}
