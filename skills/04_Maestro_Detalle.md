# 04 — Maestro-detalle: Devoluciones y detalle

**Estado: CONCLUIDO POR EL ESTUDIANTE (2026-10-17).**

## Alcance

Módulo: **Devoluciones**. La estructura es **Devoluciones (encabezado/maestro)** y **DetalleDevolucion (detalle)**. Se usarán los formularios existentes `DevolucionesForms` y `DetalleDevolucionForms`, sin crear formularios nuevos.

El estudiante confirmó que se usará el nombre físico singular `dbo.Tbl_DetalleDevolucion`, junto con `dbo.Tbl_Devoluciones`. No usar `Tbl_DetalleDevoluciones`.

## Esquema verificado en modo de solo lectura

- `dbo.Tbl_Devoluciones`: `IDDevoluciones int NOT NULL IDENTITY` (PK), `IDPrestamo int NOT NULL` (FK a `dbo.Tbl_Prestamo.IDPrestamo`), `FechaDevolucion datetime NOT NULL`, `DiasRetraso int NULL`, `Observaciones varchar(55) NULL`, `RecibidoPor varchar(100) NULL`.
- `dbo.Tbl_DetalleDevolucion`: `IDDetalleDaevolucion int NOT NULL IDENTITY` (PK; ese es el nombre exacto de la columna), `IDDevoluciones int NOT NULL`, `Cantidad int NOT NULL`, `Multa decimal(18,0) NULL`.
- La base no declara una restricción FK desde `Tbl_DetalleDevolucion.IDDevoluciones` a `Tbl_Devoluciones.IDDevoluciones`; la aplicación debe validar la relación. No se modificará el esquema.
- `dbo.Tbl_Pagos.IDDevoluciones` sí tiene FK hacia `dbo.Tbl_Devoluciones.IDDevoluciones`; si hay pagos relacionados, SQL Server puede impedir borrar el encabezado. No se consultaron filas para saber si existen.

## Mapa tabla → atributo → control

| Tabla | Atributo | Control existente / estado observado |
|---|---|---|
| `Tbl_Devoluciones` | `IDDevoluciones` | `DevolucionesForms.txtIdDevolucion` (solo lectura) |
| `Tbl_Devoluciones` | `IDPrestamo` | `DevolucionesForms.txtIdPrestamo` (se habilita al agregar o editar) |
| `Tbl_Devoluciones` | `FechaDevolucion` | `DevolucionesForms.dtpFechaDevolucion` |
| `Tbl_Devoluciones` | `DiasRetraso` | `DevolucionesForms.txtDiasRetraso` |
| `Tbl_Devoluciones` | `Observaciones` | `DevolucionesForms.txtObservaciones` |
| `Tbl_Devoluciones` | `RecibidoPor` | `DevolucionesForms.txtRecibidoPor` |
| `Tbl_DetalleDevolucion` | `IDDetalleDaevolucion` | `DetalleDevolucionForms.txtIdDetalleDevolucion` (solo lectura; el resultado lo muestra como `IDDetalleDevolucion`) |
| `Tbl_DetalleDevolucion` | `IDDevoluciones` | `DetalleDevolucionForms.txtIdDevolucion` (solo lectura; vinculado al encabezado seleccionado) |
| `Tbl_DetalleDevolucion` | `Cantidad` | `DetalleDevolucionForms.txtMulta` (caja ubicada junto a la etiqueta visual Cantidad) |
| `Tbl_DetalleDevolucion` | `Multa` | `DetalleDevolucionForms.txtCantidad` (caja ubicada junto a la etiqueta visual Multa) |

Cada formulario contiene un `dataGridView1`: el de encabezado lista devoluciones y el del formulario de detalle lista filas de `Tbl_DetalleDevolucion`. No existe una cuadrícula de detalle incrustada en `DevolucionesForms`; los módulos son formularios separados. No se agregan controles al diseñador.

Los botones CRUD existentes son `button1` Nuevo, `button3` Editar, `button6` Guardar, `button5` Eliminar, `button4` Cancelar, `button2`/`button7` Limpiar, `btnBuscar` Buscar y `button10` limpiar búsqueda. Sus eventos se enlazan desde los constructores; los diseñadores no declaran esos eventos.

## Clases y métodos implementados

- **Entidad:** `DevolucionEntidad` y `DetalleDevolucionEntidad` representan las columnas consultadas.
- **Datos:** `DevolucionDatos.Consultar` y `ConsultarTotales`; `DetalleDevolucionDatos.ConsultarPorDevolucion` y `ConsultarTotales`. Usan ADO.NET, parámetros SQL, `SqlDataReader` y `try-catch`. La consulta de detalle filtra obligatoriamente por el ID del encabezado seleccionado.
- `DevolucionDatos.AgregarConDetalles` inserta el encabezado con `OUTPUT INSERTED.IDDevoluciones`, toma el IDENTITY y lo asigna como `IDDevoluciones` a cada fila en `Tbl_DetalleDevolucion`. Ambos pasos usan una transacción; cualquier error ejecuta `ROLLBACK`.
- `DevolucionDatos.Editar` actualiza solo el encabezado y conserva su ID, por lo que sus detalles siguen relacionados. `EliminarConDetalles` elimina primero los detalles y luego el encabezado dentro de una transacción; si la eliminación del encabezado falla (por ejemplo, por una FK de pagos), ejecuta `ROLLBACK`.
- `DetalleDevolucionDatos.Editar` y `Eliminar` limitan cada operación al ID del detalle y al ID del encabezado seleccionado. Las consultas y cambios usan parámetros SQL.
- **Negocio:** `DevolucionNegocio` valida encabezados antes de agregar o editar y coordina la eliminación del maestro con sus detalles. `DetalleDevolucionNegocio` valida cantidad/multa y expone editar/eliminar acotados al encabezado actual.
- **Presentación:** `DevolucionesForms` conecta Buscar, Editar, Eliminar, Limpiar y Cancelar con los botones existentes. Editar conserva el ID maestro; Eliminar confirma el ID y elimina el conjunto en transacción. `DetalleDevolucionForms` habilita Buscar, Editar, Eliminar, Limpiar y Cancelar al consultar un encabezado existente. En el modo de alta mantiene los detalles en memoria hasta confirmar la transacción del encabezado.

### Paginación y totales

- Se muestran **10 registros por página** en cada cuadrícula, con navegación anterior/siguiente creada en Presentación. El filtro y la selección de otro encabezado regresan a la primera página. No se modificaron archivos `.Designer.cs` ni `.resx`.
- La cuadrícula del encabezado muestra las columnas Código devolución, Código préstamo, Fecha de devolución, Días de retraso, Observaciones y Recibido por; fecha en formato `dd/MM/yyyy HH:mm`.
- La cuadrícula del detalle muestra Código detalle, Código devolución, Cantidad devuelta y Multa; cantidades y multa se presentan con separador de miles y sin decimales, de acuerdo con el tipo `decimal(18,0)` de la columna Multa.
- Las etiquetas se alimentan desde consultas globales separadas de la lista paginada. En Devoluciones son `label12` (total), `label3` (con retraso) y `label6` (días igual a cero); los valores nulos de `DiasRetraso` no se asignan a ninguna de esas dos categorías. En Detalle son `label12` (suma de cantidades), `label3` (cantidades en filas con multa mayor a cero) y `label6` (cantidades en filas con multa nula o cero).
- Cambiar de página no modifica ni recalcula los totales.

El alcance autorizado incluye Buscar, Editar, Eliminar, Limpiar y Cancelar, además de la consulta y el alta transaccional previamente implementadas. Los archivos `.Designer.cs` y `.resx` no se modificaron. La paginación sigue siendo solo de Presentación; los totales se consultan aparte. La solución compiló correctamente y el estudiante confirmó que probó el módulo y lo considera bien; posteriormente solicitó bloquear la escritura directa en campos y, tras probarlo, dio por concluido este documento. No se modificó el esquema. El documento queda cerrado hasta que el estudiante solicite reabrirlo.
