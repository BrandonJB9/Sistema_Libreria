namespace Entidad
{
    /// <summary>
    /// Representa los campos de dbo.Tbl_Usuario y el nombre de su sucursal para mostrarlo.
    /// </summary>
    public class Usuario
    {
        public int IDUsuario { get; set; }
        public int IDCodigoSucursal { get; set; }
        public string NombreSucursal { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
        public string Telefono { get; set; }
        public bool Estado { get; set; }
    }
}
