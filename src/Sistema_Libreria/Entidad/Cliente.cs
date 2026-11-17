namespace Entidad
{
    /// <summary>
    /// Representa un registro de dbo.Tbl_Cliente.
    /// </summary>
    public class Cliente
    {
        public int IDCliente { get; set; }
        public string NombreCliente { get; set; }
        public string Telefono { get; set; }
        public string CorreoElectronico { get; set; }
    }
}
