using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Reglas sencillas de consulta para el encabezado de devoluciones.</summary>
    public class DevolucionNegocio
    {
        private readonly DevolucionDatos datos = new DevolucionDatos();

        public List<DevolucionEntidad> Consultar(string filtro)
        {
            try { return datos.Consultar(filtro); }
            catch (Exception ex) { throw new ApplicationException("No fue posible obtener las devoluciones. " + ex.Message, ex); }
        }

        public int[] ConsultarTotales()
        {
            try { return datos.ConsultarTotales(); }
            catch (Exception ex) { throw new ApplicationException("No fue posible obtener los totales de devolución. " + ex.Message, ex); }
        }

        public int AgregarConDetalles(DevolucionEntidad encabezado,
            IList<DetalleDevolucionEntidad> detalles)
        {
            ValidarEncabezado(encabezado);
            if (detalles == null || detalles.Count == 0)
                throw new ArgumentException("Agrega al menos una fila de detalle antes de guardar.");

            var negocioDetalle = new DetalleDevolucionNegocio();
            foreach (DetalleDevolucionEntidad detalle in detalles)
                negocioDetalle.ValidarParaNuevaDevolucion(detalle);

            try { return datos.AgregarConDetalles(encabezado, detalles); }
            catch (Exception ex) { throw new ApplicationException("No fue posible guardar el encabezado con sus detalles. " + ex.Message, ex); }
        }

        public void Editar(DevolucionEntidad encabezado)
        {
            ValidarEncabezado(encabezado);
            if (encabezado.IDDevoluciones <= 0)
                throw new ArgumentException("Selecciona una devolución válida para editar.");
            try { datos.Editar(encabezado); }
            catch (Exception ex) { throw new ApplicationException("No fue posible actualizar la devolución. " + ex.Message, ex); }
        }

        public void Eliminar(int idDevolucion)
        {
            if (idDevolucion <= 0)
                throw new ArgumentException("Selecciona una devolución válida para eliminar.");
            try { datos.EliminarConDetalles(idDevolucion); }
            catch (Exception ex) { throw new ApplicationException("No fue posible eliminar la devolución y sus detalles. " + ex.Message, ex); }
        }

        private static void ValidarEncabezado(DevolucionEntidad encabezado)
        {
            if (encabezado == null) throw new ArgumentNullException("encabezado");
            if (encabezado.IDPrestamo <= 0) throw new ArgumentException("Escribe un código de préstamo válido.");
            if (encabezado.DiasRetraso.HasValue && encabezado.DiasRetraso.Value < 0)
                throw new ArgumentException("Los días de retraso no pueden ser negativos.");
            if (encabezado.Observaciones != null && encabezado.Observaciones.Length > 55)
                throw new ArgumentException("Las observaciones admiten hasta 55 caracteres.");
            if (encabezado.RecibidoPor != null && encabezado.RecibidoPor.Length > 100)
                throw new ArgumentException("El nombre de quien recibe admite hasta 100 caracteres.");
        }
    }
}
