using System;
using System.Collections.Generic;
using Datos;
using Entidad;

namespace Negocio
{
    /// <summary>
    /// Coordina la consulta de sucursales entre Presentación y Datos.
    /// </summary>
    public class SucursalesNegocio
    {
        private readonly SucursalesDatos datos = new SucursalesDatos();

        public int AgregarSucursal(Sucursal sucursal)
        {
            try
            {
                ValidarSucursal(sucursal);
                return datos.AgregarSucursal(sucursal);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible agregar la sucursal. " + ex.Message, ex);
            }
        }

        public void EditarSucursal(Sucursal sucursal)
        {
            try
            {
                ValidarSucursal(sucursal);
                if (!datos.EditarSucursal(sucursal))
                {
                    throw new InvalidOperationException("No se encontró la sucursal seleccionada.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible editar la sucursal. " + ex.Message, ex);
            }
        }

        public void EliminarSucursal(int idCodigoSucursal)
        {
            try
            {
                if (idCodigoSucursal <= 0)
                {
                    throw new ArgumentException("Selecciona una sucursal válida para eliminar.");
                }

                if (!datos.EliminarSucursal(idCodigoSucursal))
                {
                    throw new InvalidOperationException("No se encontró la sucursal seleccionada.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible eliminar la sucursal. " + ex.Message, ex);
            }
        }

        public List<Sucursal> BuscarSucursales(string criterio)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(criterio))
                {
                    return datos.ConsultarSucursales();
                }

                return datos.BuscarSucursales(criterio.Trim());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible buscar sucursales.", ex);
            }
        }

        public List<Sucursal> ConsultarSucursales()
        {
            try
            {
                return datos.ConsultarSucursales();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No fue posible obtener las sucursales.", ex);
            }
        }

        private void ValidarTexto(string valor, string campo, int longitudMaxima)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ArgumentException("El campo " + campo + " es obligatorio.");
            }

            if (valor.Trim().Length > longitudMaxima)
            {
                throw new ArgumentException(
                    "El campo " + campo + " no puede superar " + longitudMaxima + " caracteres.");
            }
        }

        private void ValidarSucursal(Sucursal sucursal)
        {
            if (sucursal == null)
            {
                throw new ArgumentNullException("sucursal");
            }

            // La tabla requiere todos estos datos y limita sus longitudes.
            ValidarTexto(sucursal.NombreSucursal, "Nombre de sucursal", 100);
            ValidarTexto(sucursal.Departamento, "Departamento", 50);
            ValidarTexto(sucursal.Municipio, "Municipio", 50);
            ValidarTexto(sucursal.Direccion, "Dirección", 100);
            ValidarTexto(sucursal.Telefono, "Teléfono", 15);
        }
    }
}
