# Conexión y esquema de la base de datos

**Estado:** esquema verificado dentro del contenedor Docker. La conexión desde el cliente Windows continúa pendiente por un error de cifrado ODBC.

## Conexión verificada

- Servidor publicado en el host: `localhost,1434`
- Contenedor: `sql_prog2b` (`mcr.microsoft.com/mssql/server:2022-latest`)
- Mapeo: TCP 1434 del host → TCP 1433 del contenedor
- Base de datos: `libreriaProyecto`
- Autenticación: SQL Server; el usuario proporcionado pudo iniciar sesión
- El servidor presenta un certificado autofirmado. La consulta se realizó dentro del contenedor con confianza temporal en dicho certificado.

La contraseña no se almacena aquí ni en otros archivos compartidos. `Contraseña` aparece más adelante únicamente como el nombre verificado de una columna de la tabla `dbo.Tbl_Usuario`; no se documentan valores.

## Tablas y columnas

Todos los objetos listados pertenecen al esquema `dbo`. `NULL` indica que la columna permite nulos; `NOT NULL` indica que no los permite. `PK` marca la columna de clave primaria. Las referencias `FK` se detallan en la sección siguiente.

### `dbo.Tbl_Autor`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDAutor` | `int` | NOT NULL | PK |
| `IDLibro` | `int` | NOT NULL | FK → `dbo.Tbl_Libros.IDLibro` |
| `NombreAutor` | `varchar(100)` | NOT NULL | |
| `Nacionalidad` | `varchar(50)` | NULL | |
| `Biografia` | `varchar(100)` | NOT NULL | |
| `FechaNacimiento` | `datetime` | NULL | |

### `dbo.Tbl_Categoria`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDCategoria` | `int` | NOT NULL | PK |
| `IDLibro` | `int` | NOT NULL | FK → `dbo.Tbl_Libros.IDLibro` |
| `Categoria` | `varchar(50)` | NOT NULL | |
| `Descripcion` | `varchar(100)` | NULL | |
| `Estado` | `bit` | NOT NULL | |

### `dbo.Tbl_Cliente`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDCliente` | `int` | NOT NULL | PK |
| `NombreCliente` | `varchar(100)` | NOT NULL | |
| `Telefono` | `varchar(15)` | NOT NULL | |
| `CorreoElectronico` | `varchar(50)` | NULL | |

### `dbo.Tbl_DetalleDevolucion`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDDetalleDaevolucion` | `int` | NOT NULL | PK, IDENTITY |
| `IDDevoluciones` | `int` | NOT NULL | Sin FK declarada hacia `dbo.Tbl_Devoluciones` |
| `Cantidad` | `int` | NOT NULL | |
| `Multa` | `decimal(18,0)` | NULL | |

### `dbo.Tbl_DetallePedido`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDDetallePedido` | `int` | NOT NULL | PK |
| `IDPedido` | `int` | NOT NULL | FK → `dbo.Tbl_Pedidos.IDPedido` |
| `Cantidad` | `int` | NOT NULL | |
| `Precio` | `decimal(18,0)` | NOT NULL | |
| `Observaciones` | `varchar(100)` | NULL | |
| `Total` | `decimal(18,0)` | NOT NULL | |

### `dbo.Tbl_DetallePrestamo`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDDetallePrestamo` | `int` | NOT NULL | PK |
| `IDPrestamo` | `int` | NOT NULL | FK → `dbo.Tbl_Prestamo.IDPrestamo` |
| `Cantidad` | `int` | NOT NULL | |
| `Precio` | `decimal(18,0)` | NOT NULL | |
| `Descuento` | `decimal(18,0)` | NULL | |
| `Total` | `decimal(18,0)` | NOT NULL | |

### `dbo.Tbl_Devoluciones`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDDevoluciones` | `int` | NOT NULL | PK, IDENTITY |
| `IDPrestamo` | `int` | NOT NULL | FK → `dbo.Tbl_Prestamo.IDPrestamo` |
| `FechaDevolucion` | `datetime` | NOT NULL | |
| `DiasRetraso` | `int` | NULL | |
| `Observaciones` | `varchar(55)` | NULL | |
| `RecibidoPor` | `varchar(100)` | NULL | |

### `dbo.Tbl_Editorial`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDEditorial` | `int` | NOT NULL | PK |
| `IDLibro` | `int` | NOT NULL | FK → `dbo.Tbl_Libros.IDLibro` |
| `NombreEditorial` | `varchar(100)` | NOT NULL | |
| `Pais` | `varchar(50)` | NOT NULL | |
| `Direccion` | `varchar(100)` | NULL | |
| `Estado` | `bit` | NOT NULL | |

### `dbo.Tbl_Libros`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDLibro` | `int` | NOT NULL | PK |
| `IDPedido` | `int` | NOT NULL | FK → `dbo.Tbl_Pedidos.IDPedido` |
| `Titulo` | `varchar(100)` | NOT NULL | |
| `ISBN` | `varchar(50)` | NOT NULL | |
| `AñoPublicacion` | `datetime` | NULL | |
| `Precio` | `decimal(18,0)` | NOT NULL | |
| `Estado` | `bit` | NULL | |

### `dbo.Tbl_Pagos`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDagos` | `int` | NOT NULL | PK |
| `TipoPago` | `varchar(50)` | NOT NULL | |
| `TipoServicio` | `varchar(50)` | NOT NULL | |
| `Comprobante` | `varchar(50)` | NOT NULL | |
| `IDPedido` | `int` | NOT NULL | FK → `dbo.Tbl_Pedidos.IDPedido` |
| `IDPrestamo` | `int` | NOT NULL | FK → `dbo.Tbl_Prestamo.IDPrestamo` |
| `IDDevoluciones` | `int` | NOT NULL | FK → `dbo.Tbl_Devoluciones.IDDevoluciones` |

### `dbo.Tbl_Pedidos`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDPedido` | `int` | NOT NULL | PK |
| `IDCodigoSucursal` | `int` | NOT NULL | FK → `dbo.Tbl_Sucursales.IDCodigoSucursal` |
| `IDCliente` | `int` | NOT NULL | FK → `dbo.Tbl_Cliente.IDCliente` |
| `TipoPedido` | `varchar(100)` | NOT NULL | |
| `FechaPedido` | `datetime` | NOT NULL | |
| `Estado` | `bit` | NOT NULL | |

### `dbo.Tbl_Prestamo`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDPrestamo` | `int` | NOT NULL | PK |
| `IDCliente` | `int` | NOT NULL | FK → `dbo.Tbl_Cliente.IDCliente` |
| `IDCodigoSucursal` | `int` | NOT NULL | FK → `dbo.Tbl_Sucursales.IDCodigoSucursal` |
| `FechaPrestamo` | `datetime` | NOT NULL | |
| `DiasPrestamo` | `int` | NOT NULL | |
| `Observaciones` | `varchar(50)` | NULL | |
| `Estado` | `bit` | NOT NULL | |

### `dbo.Tbl_Sucursales`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDCodigoSucursal` | `int` | NOT NULL | PK |
| `NombreSucursal` | `varchar(100)` | NOT NULL | |
| `Departamento` | `varchar(50)` | NOT NULL | |
| `Municipio` | `varchar(50)` | NOT NULL | |
| `Direccion` | `varchar(100)` | NOT NULL | |
| `Telefono` | `varchar(15)` | NOT NULL | |
| `HorarioAtencion` | `datetime` | NOT NULL | |
| `Estado` | `bit` | NOT NULL | |

### `dbo.Tbl_Usuario`

| Columna | Tipo | Nulabilidad | Clave |
|---|---|---|---|
| `IDUsuario` | `int` | NOT NULL | PK |
| `IDCodigoSucursal` | `int` | NOT NULL | FK → `dbo.Tbl_Sucursales.IDCodigoSucursal` |
| `NombreUsuario` | `varchar(50)` | NOT NULL | |
| `Contraseña` | `varchar(50)` | NOT NULL | |
| `Telefono` | `varchar(15)` | NOT NULL | |
| `Estado` | `bit` | NOT NULL | |

## Restricciones de clave foránea verificadas

| Restricción | Columna de origen | Columna referenciada |
|---|---|---|
| `FK__Tbl_Autor__Fecha__4AB81AF0` | `dbo.Tbl_Autor.IDLibro` | `dbo.Tbl_Libros.IDLibro` |
| `FK__Tbl_Categ__Estad__47DBAE45` | `dbo.Tbl_Categoria.IDLibro` | `dbo.Tbl_Libros.IDLibro` |
| `FK__Tbl_Detal__IDPed__4D94879B` | `dbo.Tbl_DetallePedido.IDPedido` | `dbo.Tbl_Pedidos.IDPedido` |
| `FK__Tbl_Detal__IDPre__5441852A` | `dbo.Tbl_DetallePrestamo.IDPrestamo` | `dbo.Tbl_Prestamo.IDPrestamo` |
| `FK__Tbl_Devol__IDPre__571DF1D5` | `dbo.Tbl_Devoluciones.IDPrestamo` | `dbo.Tbl_Prestamo.IDPrestamo` |
| `FK__Tbl_Edito__IDLib__44FF419A` | `dbo.Tbl_Editorial.IDLibro` | `dbo.Tbl_Libros.IDLibro` |
| `FK__Tbl_Libro__IDPed__4222D4EF` | `dbo.Tbl_Libros.IDPedido` | `dbo.Tbl_Pedidos.IDPedido` |
| `FK__Tbl_Pagos__IDPre__5CD6CB2B` | `dbo.Tbl_Pagos.IDPrestamo` | `dbo.Tbl_Prestamo.IDPrestamo` |
| `FK__Tbl_Pagos__IDDev__5DCAEF64` | `dbo.Tbl_Pagos.IDDevoluciones` | `dbo.Tbl_Devoluciones.IDDevoluciones` |
| `FK__Tbl_Pagos__IDPed__5BE2A6F2` | `dbo.Tbl_Pagos.IDPedido` | `dbo.Tbl_Pedidos.IDPedido` |
| `FK__Tbl_Pedid__IDCli__3F466844` | `dbo.Tbl_Pedidos.IDCliente` | `dbo.Tbl_Cliente.IDCliente` |
| `FK__Tbl_Pedid__IDCod__3E52440B` | `dbo.Tbl_Pedidos.IDCodigoSucursal` | `dbo.Tbl_Sucursales.IDCodigoSucursal` |
| `FK__Tbl_Prest__IDCod__5165187F` | `dbo.Tbl_Prestamo.IDCodigoSucursal` | `dbo.Tbl_Sucursales.IDCodigoSucursal` |
| `FK__Tbl_Prest__IDCli__5070F446` | `dbo.Tbl_Prestamo.IDCliente` | `dbo.Tbl_Cliente.IDCliente` |
| `FK__Tbl_Usuar__IDCod__398D8EEE` | `dbo.Tbl_Usuario.IDCodigoSucursal` | `dbo.Tbl_Sucursales.IDCodigoSucursal` |

## Método y alcance

Los metadatos se obtuvieron del catálogo de SQL Server (`sys.tables`, `sys.columns`, `sys.types`, `sys.key_constraints`, `sys.index_columns` y `sys.foreign_key_columns`) con consultas de solo lectura sobre `libreriaProyecto`. Se verificaron 14 tablas, sus columnas, nulabilidad, claves primarias y 15 restricciones de clave foránea. No se consultaron filas de datos ni se modificó el esquema.

El 2026-10-16 se volvió a consultar solo el catálogo para confirmar `is_identity` y las restricciones relacionadas con `dbo.Tbl_Devoluciones` y `dbo.Tbl_DetalleDevolucion`. Ambas PK son IDENTITY. `Tbl_Devoluciones.IDPrestamo` referencia `Tbl_Prestamo.IDPrestamo`; no existe una FK declarada desde `Tbl_DetalleDevolucion.IDDevoluciones` hacia `Tbl_Devoluciones.IDDevoluciones`. No se consultaron datos ni se modificó el esquema.
