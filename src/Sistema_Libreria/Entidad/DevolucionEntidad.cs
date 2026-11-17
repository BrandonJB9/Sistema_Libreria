namespace Entidad
{
    public class DevolucionEntidad
    {
        public int IDDevoluciones { get; set; }
        public int IDPrestamo { get; set; }
        public System.DateTime FechaDevolucion { get; set; }
        public int? DiasRetraso { get; set; }
        public string Observaciones { get; set; }
        public string RecibidoPor { get; set; }
    }
}
