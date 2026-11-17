using System.Collections.Generic;
using System;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Coordina la consulta de préstamos.</summary>
    public class PrestamoNegocio
    {
        private readonly PrestamoDatos prestamoDatos = new PrestamoDatos();

        public List<Prestamo> Consultar()
        {
            return prestamoDatos.Consultar();
        }

        public List<KeyValuePair<int, string>> ConsultarClientesRelacionados()
        {
            return prestamoDatos.ConsultarClientesRelacionados();
        }

        public List<KeyValuePair<int, string>> ConsultarSucursalesRelacionadas()
        {
            return prestamoDatos.ConsultarSucursalesRelacionadas();
        }

        public int AgregarConDetalles(Prestamo prestamo, IList<DetallePrestamo> detalles)
        {
            if (prestamo == null) throw new ArgumentNullException("prestamo");
            if (prestamo.IDCliente <= 0 || prestamo.IDCodigoSucursal <= 0)
                throw new ArgumentException("Ingrese códigos válidos de cliente y sucursal.");
            if (prestamo.DiasPrestamo <= 0)
                throw new ArgumentException("Los días del préstamo deben ser mayores que cero.");
            if (prestamo.Observaciones != null && prestamo.Observaciones.Length > 50)
                throw new ArgumentException("Las observaciones no pueden superar los 50 caracteres.");
            if (detalles == null || detalles.Count == 0)
                throw new ArgumentException("Agregue al menos un detalle antes de guardar.");
            foreach (DetallePrestamo detalle in detalles)
                DetallePrestamoNegocio.ValidarDetalle(detalle);
            return prestamoDatos.AgregarConDetalles(prestamo, detalles);
        }

        public void Eliminar(int idPrestamo)
        {
            if (idPrestamo <= 0)
                throw new ArgumentException("Selecciona un préstamo válido para eliminar.");
            prestamoDatos.EliminarConDetalles(idPrestamo);
        }

        public void Actualizar(Prestamo prestamo)
        {
            if (prestamo == null)
                throw new ArgumentNullException("prestamo");
            if (prestamo.IDPrestamo <= 0 || prestamo.IDCliente <= 0 || prestamo.IDCodigoSucursal <= 0)
                throw new ArgumentException("Selecciona un préstamo, cliente y sucursal válidos.");
            if (prestamo.DiasPrestamo <= 0)
                throw new ArgumentException("Los días del préstamo deben ser mayores que cero.");
            if (prestamo.Observaciones != null && prestamo.Observaciones.Length > 50)
                throw new ArgumentException("Las observaciones no pueden superar los 50 caracteres.");

            prestamoDatos.Actualizar(prestamo);
        }
    }
}
