using System;
using System.Collections.Generic;

namespace Entidad
{
    /// <summary>Indicadores y listados reales que muestra el dashboard.</summary>
    public sealed class DashboardResumen
    {
        public int LibrosEnCatalogo { get; set; }
        public int PedidosEnPeriodo { get; set; }
        public int PrestamosActivos { get; set; }
        public int PrestamosVencidos { get; set; }
        public decimal IngresosEnPeriodo { get; set; }
        public List<DateTime> ProximosVencimientos { get; set; } = new List<DateTime>();
        public List<DashboardMovimiento> Movimientos { get; set; } = new List<DashboardMovimiento>();
        public List<DashboardCliente> Clientes { get; set; } = new List<DashboardCliente>();
        public List<DashboardPedido> Pedidos { get; set; } = new List<DashboardPedido>();
        public List<DashboardCategoria> Categorias { get; set; } = new List<DashboardCategoria>();
        public float[] PrestamosPorDia { get; set; } = new float[0];
        public float[] DevolucionesPorDia { get; set; } = new float[0];
        public float[] IngresosPorMes { get; set; } = new float[0];
        public string[] EtiquetasMeses { get; set; } = new string[0];
    }

    public sealed class DashboardMovimiento
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
        public string Tipo { get; set; }
    }

    public sealed class DashboardCliente
    {
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
    }

    public sealed class DashboardPedido
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
    }

    public sealed class DashboardCategoria
    {
        public string Nombre { get; set; }
        public int Disponibles { get; set; }
        public int NoDisponibles { get; set; }
    }
}
