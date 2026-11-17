using System;

namespace Entidad
{
    /// <summary>
    /// Representa una fila de dbo.Tbl_Sucursales.
    /// </summary>
    public class Sucursal
    {
        public int IDCodigoSucursal { get; set; }
        public string NombreSucursal { get; set; }
        public string Departamento { get; set; }
        public string Municipio { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public DateTime HorarioAtencion { get; set; }
        public bool Estado { get; set; }
    }
}
