using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidad;

namespace Datos
{
    /// <summary>Consultas de solo lectura para los indicadores del dashboard.</summary>
    public sealed class DashboardDatos
    {
        public List<KeyValuePair<int, string>> ConsultarSucursales()
        {
            const string sql = "SELECT IDCodigoSucursal, NombreSucursal FROM dbo.Tbl_Sucursales ORDER BY NombreSucursal;";
            var resultado = new List<KeyValuePair<int, string>>();
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                        while (lector.Read())
                            resultado.Add(new KeyValuePair<int, string>(lector.GetInt32(0), lector.GetString(1)));
                }
                return resultado;
            }
            catch (Exception ex) { throw new InvalidOperationException("No se pudieron cargar las sucursales del dashboard.", ex); }
        }

        public DashboardResumen Consultar(DateTime desde, DateTime hasta, int? idSucursal, int diasGrafica, bool todosLosMovimientos = false)
        {
            if (desde >= hasta) throw new ArgumentException("El periodo del dashboard no es válido.");
            if (diasGrafica != 15 && diasGrafica != 30) throw new ArgumentOutOfRangeException("diasGrafica");

            const string resumenSql = @"
                SELECT
                    (SELECT COUNT(*) FROM dbo.Tbl_Libros l
                     INNER JOIN dbo.Tbl_Pedidos lp ON lp.IDPedido = l.IDPedido
                     WHERE (@Sucursal IS NULL OR lp.IDCodigoSucursal = @Sucursal)) AS Libros,
                    (SELECT COUNT(*) FROM dbo.Tbl_Pedidos p
                     WHERE p.FechaPedido >= @Desde AND p.FechaPedido < @Hasta
                       AND (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)) AS Pedidos,
                    (SELECT COUNT(*) FROM dbo.Tbl_Prestamo p
                     WHERE p.Estado = 1
                       AND NOT EXISTS (SELECT 1 FROM dbo.Tbl_Devoluciones d WHERE d.IDPrestamo = p.IDPrestamo)
                       AND (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)) AS PrestamosActivos,
                    (SELECT COUNT(*) FROM dbo.Tbl_Prestamo p
                     WHERE p.Estado = 1
                       AND DATEADD(day, p.DiasPrestamo, p.FechaPrestamo) < CONVERT(date, GETDATE())
                       AND NOT EXISTS (SELECT 1 FROM dbo.Tbl_Devoluciones d WHERE d.IDPrestamo = p.IDPrestamo)
                       AND (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)) AS PrestamosVencidos,
                    ISNULL((SELECT SUM(dp.Total) FROM dbo.Tbl_DetallePedido dp
                            INNER JOIN dbo.Tbl_Pedidos p ON p.IDPedido = dp.IDPedido
                            WHERE p.FechaPedido >= @Desde AND p.FechaPedido < @Hasta
                              AND (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)), 0)
                    + ISNULL((SELECT SUM(dd.Total) FROM dbo.Tbl_DetallePrestamo dd
                              INNER JOIN dbo.Tbl_Prestamo p ON p.IDPrestamo = dd.IDPrestamo
                              WHERE p.FechaPrestamo >= @Desde AND p.FechaPrestamo < @Hasta
                                AND (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)), 0)
                    + ISNULL((SELECT SUM(ISNULL(det.Multa, 0)) FROM dbo.Tbl_DetalleDevolucion det
                              INNER JOIN dbo.Tbl_Devoluciones d ON d.IDDevoluciones = det.IDDevoluciones
                              INNER JOIN dbo.Tbl_Prestamo p ON p.IDPrestamo = d.IDPrestamo
                              WHERE d.FechaDevolucion >= @Desde AND d.FechaDevolucion < @Hasta
                                AND (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)), 0) AS Ingresos;";

            var resumen = new DashboardResumen();
            DateTime hoy = DateTime.Today;
            DateTime desdeGrafica = hoy.AddDays(-(diasGrafica - 1));
            DateTime hastaGrafica = hoy.AddDays(1);
            try
            {
                using (SqlConnection conexion = Conexion.Crear())
                {
                    conexion.Open();
                    using (SqlCommand comando = CrearComando(conexion, resumenSql, desde, hasta, idSucursal))
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        if (!lector.Read()) throw new DataException("La consulta de indicadores no devolvió resultados.");
                        resumen.LibrosEnCatalogo = Convert.ToInt32(lector["Libros"]);
                        resumen.PedidosEnPeriodo = Convert.ToInt32(lector["Pedidos"]);
                        resumen.PrestamosActivos = Convert.ToInt32(lector["PrestamosActivos"]);
                        resumen.PrestamosVencidos = Convert.ToInt32(lector["PrestamosVencidos"]);
                        resumen.IngresosEnPeriodo = Convert.ToDecimal(lector["Ingresos"]);
                    }

                    resumen.ProximosVencimientos = ConsultarProximosVencimientos(conexion, hoy, idSucursal);
                    resumen.Movimientos = ConsultarMovimientos(conexion, idSucursal, todosLosMovimientos);
                    resumen.Clientes = ConsultarClientes(conexion, idSucursal);
                    resumen.Pedidos = ConsultarPedidos(conexion, idSucursal);
                    resumen.Categorias = ConsultarCategorias(conexion, idSucursal);
                    ConsultarSeries(conexion, resumen, desdeGrafica, hastaGrafica, diasGrafica, idSucursal);
                    ConsultarIngresosMensuales(conexion, resumen, idSucursal);
                }
                return resumen;
            }
            catch (Exception ex) { throw new InvalidOperationException("No se pudieron cargar los datos reales del dashboard.", ex); }
        }

        static SqlCommand CrearComando(SqlConnection conexion, string sql, DateTime desde, DateTime hasta, int? idSucursal)
        {
            var comando = new SqlCommand(sql, conexion);
            comando.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde;
            comando.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = hasta;
            comando.Parameters.Add("@Sucursal", SqlDbType.Int).Value = idSucursal.HasValue ? (object)idSucursal.Value : DBNull.Value;
            return comando;
        }

        static List<DateTime> ConsultarProximosVencimientos(SqlConnection conexion, DateTime hoy, int? sucursal)
        {
            const string sql = @"SELECT TOP 3 DATEADD(day, p.DiasPrestamo, p.FechaPrestamo) AS Vence
                FROM dbo.Tbl_Prestamo p
                WHERE p.Estado = 1 AND DATEADD(day, p.DiasPrestamo, p.FechaPrestamo) >= @Hoy
                  AND NOT EXISTS (SELECT 1 FROM dbo.Tbl_Devoluciones d WHERE d.IDPrestamo = p.IDPrestamo)
                  AND (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)
                ORDER BY Vence;";
            var resultado = new List<DateTime>();
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Hoy", SqlDbType.Date).Value = hoy;
                comando.Parameters.Add("@Sucursal", SqlDbType.Int).Value = sucursal.HasValue ? (object)sucursal.Value : DBNull.Value;
                using (SqlDataReader lector = comando.ExecuteReader())
                    while (lector.Read()) resultado.Add(lector.GetDateTime(0));
            }
            return resultado;
        }

        static List<DashboardMovimiento> ConsultarMovimientos(SqlConnection conexion, int? sucursal, bool todosLosMovimientos)
        {
            const string sql = @"SELECT TOP (@Limite) Id, Fecha, Unidades, Tipo FROM (
                    SELECT p.IDPedido AS Id, p.FechaPedido AS Fecha, ISNULL(SUM(d.Cantidad), 0) AS Unidades, 'Pedido' AS Tipo
                    FROM dbo.Tbl_Pedidos p LEFT JOIN dbo.Tbl_DetallePedido d ON d.IDPedido = p.IDPedido
                    WHERE (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)
                    GROUP BY p.IDPedido, p.FechaPedido
                    UNION ALL
                    SELECT p.IDPrestamo, p.FechaPrestamo, ISNULL(SUM(d.Cantidad), 0), 'Préstamo'
                    FROM dbo.Tbl_Prestamo p LEFT JOIN dbo.Tbl_DetallePrestamo d ON d.IDPrestamo = p.IDPrestamo
                    WHERE (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)
                    GROUP BY p.IDPrestamo, p.FechaPrestamo
                ) AS Movimientos ORDER BY Fecha DESC, Id DESC;";
            var resultado = new List<DashboardMovimiento>();
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Limite", SqlDbType.Int).Value = todosLosMovimientos ? int.MaxValue : 9;
                comando.Parameters.Add("@Sucursal", SqlDbType.Int).Value = sucursal.HasValue ? (object)sucursal.Value : DBNull.Value;
                using (SqlDataReader lector = comando.ExecuteReader())
                    while (lector.Read()) resultado.Add(new DashboardMovimiento { Id = lector.GetInt32(0), Fecha = lector.GetDateTime(1), Cantidad = Convert.ToInt32(lector[2]), Tipo = lector.GetString(3) });
            }
            return resultado;
        }

        static List<DashboardCliente> ConsultarClientes(SqlConnection conexion, int? sucursal)
        {
            const string sql = @"SELECT TOP 5 NombreCliente, Telefono, CorreoElectronico
                FROM dbo.Tbl_Cliente c
                WHERE (@Sucursal IS NULL
                    OR EXISTS (SELECT 1 FROM dbo.Tbl_Pedidos p WHERE p.IDCliente = c.IDCliente AND p.IDCodigoSucursal = @Sucursal)
                    OR EXISTS (SELECT 1 FROM dbo.Tbl_Prestamo pr WHERE pr.IDCliente = c.IDCliente AND pr.IDCodigoSucursal = @Sucursal))
                ORDER BY IDCliente DESC;";
            var resultado = new List<DashboardCliente>();
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Sucursal", SqlDbType.Int).Value = sucursal.HasValue ? (object)sucursal.Value : DBNull.Value;
                using (SqlDataReader lector = comando.ExecuteReader())
                    while (lector.Read()) resultado.Add(new DashboardCliente
                    {
                        Nombre = lector.GetString(0),
                        Telefono = lector.GetString(1),
                        Correo = lector.IsDBNull(2) ? "Sin correo" : lector.GetString(2)
                    });
            }
            return resultado;
        }

        static List<DashboardPedido> ConsultarPedidos(SqlConnection conexion, int? sucursal)
        {
            const string sql = @"SELECT TOP 5 p.IDPedido, p.FechaPedido, c.NombreCliente,
                    ISNULL(SUM(d.Total), 0) AS Total,
                    CASE WHEN p.Estado = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado
                FROM dbo.Tbl_Pedidos p
                INNER JOIN dbo.Tbl_Cliente c ON c.IDCliente = p.IDCliente
                LEFT JOIN dbo.Tbl_DetallePedido d ON d.IDPedido = p.IDPedido
                WHERE (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)
                GROUP BY p.IDPedido, p.FechaPedido, c.NombreCliente, p.Estado
                ORDER BY p.FechaPedido DESC, p.IDPedido DESC;";
            var resultado = new List<DashboardPedido>();
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Sucursal", SqlDbType.Int).Value = sucursal.HasValue ? (object)sucursal.Value : DBNull.Value;
                using (SqlDataReader lector = comando.ExecuteReader())
                    while (lector.Read()) resultado.Add(new DashboardPedido
                {
                    Id = lector.GetInt32(0), Fecha = lector.GetDateTime(1), Cliente = lector.GetString(2),
                    Total = Convert.ToDecimal(lector[3]), Estado = lector.GetString(4)
                });
            }
            return resultado;
        }

        static List<DashboardCategoria> ConsultarCategorias(SqlConnection conexion, int? sucursal)
        {
            const string sql = @"SELECT TOP 7 c.Categoria,
                    COUNT(DISTINCT CASE WHEN l.Estado = 1 THEN c.IDLibro END) AS Disponibles,
                    COUNT(DISTINCT CASE WHEN l.Estado = 0 THEN c.IDLibro END) AS NoDisponibles
                FROM dbo.Tbl_Categoria c INNER JOIN dbo.Tbl_Libros l ON l.IDLibro = c.IDLibro
                INNER JOIN dbo.Tbl_Pedidos p ON p.IDPedido = l.IDPedido
                WHERE (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)
                GROUP BY c.Categoria
                ORDER BY COUNT(DISTINCT c.IDLibro) DESC, c.Categoria;";
            var resultado = new List<DashboardCategoria>();
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Sucursal", SqlDbType.Int).Value = sucursal.HasValue ? (object)sucursal.Value : DBNull.Value;
            using (SqlDataReader lector = comando.ExecuteReader())
                while (lector.Read()) resultado.Add(new DashboardCategoria
                {
                    Nombre = lector.GetString(0), Disponibles = Convert.ToInt32(lector[1]), NoDisponibles = Convert.ToInt32(lector[2])
                });
            }
            return resultado;
        }

        static void ConsultarSeries(SqlConnection conexion, DashboardResumen resumen, DateTime desde, DateTime hasta, int dias, int? sucursal)
        {
            const string prestamosSql = @"SELECT CONVERT(date, p.FechaPrestamo), COUNT(*)
                FROM dbo.Tbl_Prestamo p WHERE p.FechaPrestamo >= @Desde AND p.FechaPrestamo < @Hasta
                  AND (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)
                GROUP BY CONVERT(date, p.FechaPrestamo);";
            const string devolucionesSql = @"SELECT CONVERT(date, d.FechaDevolucion), COUNT(*)
                FROM dbo.Tbl_Devoluciones d INNER JOIN dbo.Tbl_Prestamo p ON p.IDPrestamo = d.IDPrestamo
                WHERE d.FechaDevolucion >= @Desde AND d.FechaDevolucion < @Hasta
                  AND (@Sucursal IS NULL OR p.IDCodigoSucursal = @Sucursal)
                GROUP BY CONVERT(date, d.FechaDevolucion);";
            resumen.PrestamosPorDia = new float[dias];
            resumen.DevolucionesPorDia = new float[dias];
            CargarSerie(conexion, prestamosSql, resumen.PrestamosPorDia, desde, hasta, sucursal);
            CargarSerie(conexion, devolucionesSql, resumen.DevolucionesPorDia, desde, hasta, sucursal);
        }

        static void CargarSerie(SqlConnection conexion, string sql, float[] valores, DateTime desde, DateTime hasta, int? sucursal)
        {
            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Desde", SqlDbType.DateTime).Value = desde;
                comando.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = hasta;
                comando.Parameters.Add("@Sucursal", SqlDbType.Int).Value = sucursal.HasValue ? (object)sucursal.Value : DBNull.Value;
                using (SqlDataReader lector = comando.ExecuteReader())
                    while (lector.Read())
                    {
                        DateTime fecha = lector.GetDateTime(0);
                        int indice = (fecha.Date - desde.Date).Days;
                        if (indice >= 0 && indice < valores.Length) valores[indice] = Convert.ToSingle(lector[1]);
                    }
            }
        }

        static void ConsultarIngresosMensuales(SqlConnection conexion, DashboardResumen resumen, int? sucursal)
        {
            DateTime primerMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-7);
            DateTime fin = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1);
            const string sql = @"SELECT YEAR(Fecha) AS Anio, MONTH(Fecha) AS Mes, SUM(Monto) AS Total
                FROM (
                    SELECT p.FechaPedido AS Fecha, p.IDCodigoSucursal, d.Total AS Monto
                    FROM dbo.Tbl_Pedidos p INNER JOIN dbo.Tbl_DetallePedido d ON d.IDPedido = p.IDPedido
                    UNION ALL
                    SELECT p.FechaPrestamo, p.IDCodigoSucursal, d.Total
                    FROM dbo.Tbl_Prestamo p INNER JOIN dbo.Tbl_DetallePrestamo d ON d.IDPrestamo = p.IDPrestamo
                    UNION ALL
                    SELECT v.FechaDevolucion, p.IDCodigoSucursal, ISNULL(d.Multa, 0)
                    FROM dbo.Tbl_Devoluciones v INNER JOIN dbo.Tbl_Prestamo p ON p.IDPrestamo = v.IDPrestamo
                    INNER JOIN dbo.Tbl_DetalleDevolucion d ON d.IDDevoluciones = v.IDDevoluciones
                ) AS Operaciones
                WHERE Fecha >= @Desde AND Fecha < @Hasta
                  AND (@Sucursal IS NULL OR IDCodigoSucursal = @Sucursal)
                GROUP BY YEAR(Fecha), MONTH(Fecha);";

            resumen.IngresosPorMes = new float[8];
            resumen.EtiquetasMeses = new string[8];
            string[] nombresMeses = { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            for (int i = 0; i < 8; i++) resumen.EtiquetasMeses[i] = primerMes.AddMonths(i).Month == DateTime.Today.Month && primerMes.AddMonths(i).Year == DateTime.Today.Year
                ? "Mes" : nombresMeses[primerMes.AddMonths(i).Month - 1];

            using (var comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.Add("@Desde", SqlDbType.DateTime).Value = primerMes;
                comando.Parameters.Add("@Hasta", SqlDbType.DateTime).Value = fin;
                comando.Parameters.Add("@Sucursal", SqlDbType.Int).Value = sucursal.HasValue ? (object)sucursal.Value : DBNull.Value;
                using (SqlDataReader lector = comando.ExecuteReader())
                    while (lector.Read())
                    {
                        var mes = new DateTime(Convert.ToInt32(lector[0]), Convert.ToInt32(lector[1]), 1);
                        int indice = (mes.Year - primerMes.Year) * 12 + mes.Month - primerMes.Month;
                        if (indice >= 0 && indice < resumen.IngresosPorMes.Length)
                            resumen.IngresosPorMes[indice] = Convert.ToSingle(lector[2]);
                    }
            }
        }
    }
}
