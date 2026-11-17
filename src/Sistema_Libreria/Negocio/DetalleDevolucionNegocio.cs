using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Valida la selección y consulta solamente el detalle relacionado.</summary>
    public class DetalleDevolucionNegocio
    {
        private readonly DetalleDevolucionDatos datos = new DetalleDevolucionDatos();

        public List<DetalleDevolucionEntidad> ConsultarPorDevolucion(int idDevolucion, string filtro)
        {
            if (idDevolucion <= 0)
                throw new ArgumentException("Selecciona un encabezado de devolución válido.");

            try { return datos.ConsultarPorDevolucion(idDevolucion, filtro); }
            catch (Exception ex) { throw new ApplicationException("No fue posible consultar las filas relacionadas con la devolución. " + ex.Message, ex); }
        }

        public decimal[] ConsultarTotales()
        {
            try { return datos.ConsultarTotales(); }
            catch (Exception ex) { throw new ApplicationException("No fue posible obtener los totales del detalle. " + ex.Message, ex); }
        }

        public void ValidarParaNuevaDevolucion(DetalleDevolucionEntidad detalle)
        {
            if (detalle == null) throw new ArgumentNullException("detalle");
            if (detalle.Cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que cero.");
            if (detalle.Multa.HasValue && detalle.Multa.Value < 0)
                throw new ArgumentException("La multa no puede ser negativa.");
            if (detalle.Multa.HasValue && detalle.Multa.Value != decimal.Truncate(detalle.Multa.Value))
                throw new ArgumentException("La multa solo admite valores enteros según el esquema actual.");
        }

        public void Editar(DetalleDevolucionEntidad detalle)
        {
            if (detalle == null) throw new ArgumentNullException("detalle");
            if (detalle.IDDetalleDevolucion <= 0 || detalle.IDDevoluciones <= 0)
                throw new ArgumentException("Selecciona un detalle válido de la devolución actual.");
            ValidarParaNuevaDevolucion(detalle);
            try { datos.Editar(detalle); }
            catch (Exception ex) { throw new ApplicationException("No fue posible actualizar el detalle. " + ex.Message, ex); }
        }

        public void Eliminar(int idDevolucion, int idDetalle)
        {
            if (idDevolucion <= 0 || idDetalle <= 0)
                throw new ArgumentException("Selecciona un detalle válido de la devolución actual.");
            try { datos.Eliminar(idDevolucion, idDetalle); }
            catch (Exception ex) { throw new ApplicationException("No fue posible eliminar el detalle. " + ex.Message, ex); }
        }
    }
}
