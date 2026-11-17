using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>Valida los filtros y coordina las consultas de lectura del dashboard.</summary>
    public sealed class DashboardNegocio
    {
        private readonly DashboardDatos datos = new DashboardDatos();

        public List<KeyValuePair<int, string>> ConsultarSucursales()
        {
            return datos.ConsultarSucursales();
        }

        public DashboardResumen Consultar(DateTime desde, DateTime hasta, int? idSucursal, int diasGrafica, bool todosLosMovimientos = false)
        {
            if (idSucursal.HasValue && idSucursal.Value <= 0)
                throw new ArgumentOutOfRangeException("idSucursal");
            return datos.Consultar(desde, hasta, idSucursal, diasGrafica, todosLosMovimientos);
        }
    }
}
