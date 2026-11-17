using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Valida los datos del pago y coordina sus operaciones.</summary>
    public class PagoNegocio
    {
        private readonly PagoDatos pagoDatos = new PagoDatos();

        public List<Pago> Consultar() { return pagoDatos.Consultar(); }

        public List<KeyValuePair<int, string>> ConsultarPrestamosRelacionados()
        {
            return pagoDatos.ConsultarPrestamosRelacionados();
        }

        public List<KeyValuePair<int, string>> ConsultarDevolucionesRelacionadas()
        {
            return pagoDatos.ConsultarDevolucionesRelacionadas();
        }

        public List<Pago> Buscar(string criterio)
        {
            return string.IsNullOrWhiteSpace(criterio) ? Consultar() : pagoDatos.Buscar(criterio.Trim());
        }

        public int Agregar(Pago pago)
        {
            Validar(pago);
            ValidarRelaciones(pago);
            return pagoDatos.Agregar(pago);
        }

        public bool Editar(Pago pago)
        {
            Validar(pago);
            if (pago.IDagos <= 0)
                throw new ArgumentException("Selecciona un pago válido para editar.");
            ValidarRelaciones(pago);
            return pagoDatos.Editar(pago);
        }

        public bool Eliminar(int idPago)
        {
            if (idPago <= 0)
                throw new ArgumentException("Selecciona un pago válido para eliminar.");
            return pagoDatos.Eliminar(idPago);
        }

        private void ValidarRelaciones(Pago pago)
        {
            if (!pagoDatos.ExistenRelaciones(pago))
                throw new ArgumentException("Verifica que los códigos existan y que la devolución pertenezca al préstamo seleccionado.");
        }

        private static void Validar(Pago pago)
        {
            if (pago == null) throw new ArgumentNullException("pago");
            if (string.IsNullOrWhiteSpace(pago.TipoPago) || pago.TipoPago.Trim().Length > 50)
                throw new ArgumentException("Selecciona un tipo de pago válido (máximo 50 caracteres).");
            if (string.IsNullOrWhiteSpace(pago.TipoServicio) || pago.TipoServicio.Trim().Length > 50)
                throw new ArgumentException("Selecciona un tipo de servicio válido (máximo 50 caracteres).");
            if (string.IsNullOrWhiteSpace(pago.Comprobante) || pago.Comprobante.Trim().Length > 50)
                throw new ArgumentException("El comprobante es obligatorio y admite hasta 50 caracteres.");
            if (pago.IDPedido <= 0 || pago.IDPrestamo <= 0 || pago.IDDevoluciones <= 0)
                throw new ArgumentException("Los códigos de pedido, préstamo y devolución son obligatorios.");

            pago.TipoPago = pago.TipoPago.Trim();
            pago.TipoServicio = pago.TipoServicio.Trim();
            pago.Comprobante = pago.Comprobante.Trim();
        }
    }
}
