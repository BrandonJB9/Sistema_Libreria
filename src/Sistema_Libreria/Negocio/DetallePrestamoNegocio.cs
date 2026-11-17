using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Valida y coordina la consulta de detalles de un préstamo.</summary>
    public class DetallePrestamoNegocio
    {
        private readonly DetallePrestamoDatos detalleDatos = new DetallePrestamoDatos();

        public List<DetallePrestamo> ConsultarPorPrestamo(int idPrestamo)
        {
            if (idPrestamo <= 0)
                throw new ArgumentException("Selecciona un código de préstamo válido.");
            return detalleDatos.ConsultarPorPrestamo(idPrestamo);
        }

        public int Agregar(DetallePrestamo detalle)
        {
            ValidarDetalle(detalle);
            if (detalle.IDPrestamo <= 0)
                throw new ArgumentException("El detalle debe pertenecer a un préstamo existente.");
            return detalleDatos.Agregar(detalle);
        }

        public void Actualizar(DetallePrestamo detalle)
        {
            ValidarDetalle(detalle);
            if (detalle.IDDetallePrestamo <= 0 || detalle.IDPrestamo <= 0)
                throw new ArgumentException("Selecciona una fila de detalle válida.");
            detalleDatos.Actualizar(detalle);
        }

        public bool Eliminar(int idDetallePrestamo, int idPrestamo)
        {
            if (idDetallePrestamo <= 0 || idPrestamo <= 0)
                throw new ArgumentException("Selecciona una fila de detalle válida.");
            return detalleDatos.Eliminar(idDetallePrestamo, idPrestamo);
        }

        internal static void ValidarDetalle(DetallePrestamo detalle)
        {
            if (detalle == null)
                throw new ArgumentNullException("detalle");
            decimal totalEsperado = CalcularTotal(detalle.Cantidad, detalle.Precio, detalle.Descuento);
            if (detalle.Total != totalEsperado)
                throw new ArgumentException("El total no coincide con la cantidad, el precio y el porcentaje de descuento.");
        }

        /// <summary>Aplica un porcentaje y redondea a quetzales enteros según decimal(18,0).</summary>
        public static decimal CalcularTotal(int cantidad, decimal precio, decimal? descuento)
        {
            const decimal maximoSql = 999999999999999999m;
            decimal rebaja = descuento ?? 0m;
            if (cantidad <= 0 || precio < 0 || precio > maximoSql ||
                rebaja < 0 || rebaja > 100 || decimal.Truncate(precio) != precio ||
                decimal.Truncate(rebaja) != rebaja)
                throw new ArgumentException("Usa un precio entero no negativo y un descuento entero entre 0 y 100%.");

            decimal subtotal = cantidad * precio;
            decimal total = decimal.Round(subtotal * (1m - rebaja / 100m), 0, MidpointRounding.AwayFromZero);
            if (total > maximoSql)
                throw new ArgumentException("El total supera el importe permitido por la base de datos.");
            return total;
        }
    }
}
