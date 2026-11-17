using System;

namespace Entidad
{
    /// <summary>Datos de consulta de un préstamo.</summary>
    public class Prestamo
    {
        public int IDPrestamo { get; set; }
        public int IDCliente { get; set; }
        public int IDCodigoSucursal { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public int DiasPrestamo { get; set; }
        public string Observaciones { get; set; }
        public bool Estado { get; set; }
        public string NombreCliente { get; set; }
        public string NombreSucursal { get; set; }
    }
}
