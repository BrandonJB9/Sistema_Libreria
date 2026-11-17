# Guía de uso del Sistema Librería

## 1. Abrir la aplicación

1. En Windows, abre la solución **LibreriaProyectoProgra2.sln** de la carpeta [src/Sistema_Libreria](../src/Sistema_Libreria/LibreriaProyectoProgra2.sln) con Visual Studio y soporte para WinForms y .NET Framework 4.8.
2. Si Visual Studio solicita restaurar paquetes NuGet, acepta la restauración. Selecciona **Presentacion** como proyecto de inicio y presiona **F5**.
3. Comprueba que SQL Server tenga disponible la base de la aplicación. En otra instalación, obtén una copia de la base de quien administra el sistema y configura la conexión local en `Presentacion/App.config`. Usa como referencia el [archivo App.config.example](../src/Sistema_Libreria/Presentacion/App.config.example). Usa tus propias credenciales y mantén el archivo de configuración local fuera del repositorio.
4. En **Inicio de sesión**, escribe el usuario y la contraseña de una cuenta activa. La opción **Recordarme** guarda el nombre de usuario. Al terminar, pulsa **Cerrar sesión**.

La base de demostración configurada en este equipo tiene tres filas por tabla. Puedes consultar los ejemplos existentes desde los listados. Para practicar altas o eliminaciones sin alterar esa demostración, usa una base de práctica separada.

## 2. Orden para ingresar datos

El orden sigue las relaciones de la base. Guarda primero el registro que otro módulo necesita seleccionar. La aplicación asigna los códigos de los registros nuevos.

| Orden | Pantalla | Tabla | Requisito y uso |
| --- | --- | --- | --- |
| 1 | **Sucursales** | Tbl_Sucursales | Registra nombre, ubicación, dirección, teléfono, horario y estado. La sucursal se usa en cuentas, pedidos y préstamos. |
| 2 | **Clientes** | Tbl_Cliente | Registra nombre y teléfono. El correo es opcional. El cliente se usa en pedidos y préstamos. |
| 3 | **Usuarios** | Tbl_Usuario | Selecciona una sucursal. Crea la cuenta con un nombre de usuario, contraseña y estado. Este paso se puede omitir si ya tienes una cuenta activa para trabajar. |
| 4 | **Pedidos** | Tbl_Pedidos | Selecciona cliente y sucursal, y registra tipo, fecha y estado del pedido. |
| 5 | **Detalle de pedido** | Tbl_DetallePedido | Usa un pedido guardado. Agrega las líneas de cantidad y precio. |
| 6 | **Libros** | Tbl_Libros | Selecciona un pedido guardado. Registra el título, ISBN, precio y demás datos. El libro se vincula al pedido. |
| 7 | **Autores** | Tbl_Autor | Selecciona un libro del catálogo antes de registrar al autor. |
| 8 | **Categorías** | Tbl_Categoria | Selecciona un libro, escribe la categoría y define su estado. |
| 9 | **Editoriales** | Tbl_Editorial | Selecciona un libro y registra la editorial y su país. |
| 10 | **Préstamos** | Tbl_Prestamo | Selecciona cliente y sucursal, e indica fecha y duración. Al guardar se abre la captura de detalles. |
| 11 | **Detalle de préstamo** | Tbl_DetallePrestamo | Agrega las líneas y confirma con **Guardar préstamo**. El encabezado y las líneas se guardan juntos. |
| 12 | **Devoluciones** | Tbl_Devoluciones | Selecciona un préstamo guardado y completa los datos de devolución. Al guardar se abre la captura de detalles. |
| 13 | **Detalle de devolución** | Tbl_DetalleDevolucion | Agrega al menos una línea con cantidad y, si corresponde, multa. Pulsa **Guardar todo**. |
| 14 | **Pagos** | Tbl_Pagos | Selecciona un pedido, un préstamo y una devolución de ese préstamo. Registra el tipo de pago, tipo de servicio y comprobante. |

Puedes crear **Clientes** y **Sucursales** en cualquier orden. **Autores**, **Categorías** y **Editoriales** se pueden registrar en cualquier orden después de **Libros**. Para dar de alta solo un préstamo, basta con tener cliente y sucursal, además de los datos de sus líneas.

## 3. Usar los botones

- **Nuevo** prepara el formulario para ingresar datos. **Guardar** registra los datos nuevos o aplica la edición.
- Para modificar un registro, selecciona una fila, pulsa **Editar**, cambia los campos y guarda.
- **Limpiar** limpia los campos de captura. Para recuperar el listado completo después de una búsqueda, borra el texto del buscador y pulsa **Buscar**.
- **Cancelar** termina la captura o edición en curso sin guardarla. **Eliminar** pide confirmación antes de borrar.
- En los listados que tienen **Anterior** y **Siguiente**, usa estos botones para recorrer páginas. En **Pedidos** y **Préstamos**, un doble clic en una fila abre su detalle.
- Al editar un usuario, deja vacía la contraseña para conservar la actual. Escribe una nueva para cambiarla. Una cuenta inactiva queda fuera del inicio de sesión.

## 4. Registrar una operación completa

1. **Pedido y catálogo.** Crea un pedido para el cliente y la sucursal. Abre el detalle y registra cantidad y precio. El total de la línea es cantidad por precio. Registra el libro en **Libros** y selecciona ese pedido.
2. **Préstamo.** Abre **Préstamos**, pulsa **Nuevo**, elige al cliente y la sucursal y completa fecha, días y estado. Pulsa **Guardar**. En la captura, pulsa **Agregar detalle** por cada línea y, al terminar, **Guardar préstamo**.
3. **Devolución.** Abre **Devoluciones**, selecciona el préstamo y completa la fecha y los demás datos. Pulsa **Guardar**, registra los detalles y confirma con **Guardar todo**.
4. **Pago.** Abre **Pagos** y selecciona el pedido, el préstamo y la devolución correspondiente. Elige **Efectivo**, **Tarjeta** o **Transferencia**, indica el servicio y escribe la referencia del comprobante. Pulsa **Guardar**. El módulo registra el pago dentro del sistema.
5. **Consulta.** Busca los registros en sus listados. En **Dashboard**, usa los filtros de periodo y sucursal. El enlace **Ver todos** del listado de movimientos alterna con **Ver recientes**.

En las líneas de préstamo, el descuento es un porcentaje entero de 0 a 100. La aplicación calcula el total automáticamente. Ejemplo: dos unidades a Q100 con 25 % de descuento dan un total de **Q150**.

## 5. Límites y solución de problemas

| Si ocurre esto | Qué revisar |
| --- | --- |
| La lista de selección está vacía | Registra primero el dato requerido en la tabla de orden. Vuelve a abrir el formulario para actualizar la lista. |
| No puedes eliminar un cliente | Comprueba si tiene pedidos o préstamos. Si registraste uno con el cliente equivocado, corrige la relación donde la aplicación permita editarla. |
| No puedes eliminar un préstamo | Revisa si tiene devoluciones o pagos asociados. La aplicación protege esas relaciones al eliminar. |
| No puedes guardar un pago | Deben existir el pedido, el préstamo y su devolución. La devolución elegida debe pertenecer al préstamo que seleccionaste. |
| La contraseña se rechaza | Verifica los datos de acceso, que la cuenta esté activa y que SQL Server esté disponible. |
| El importe del Dashboard difiere de los pagos | **Importe de operaciones** agrega montos de pedidos, préstamos y multas. Consulta **Pagos** para ver los pagos registrados. |

El acceso requiere una cuenta activa. Las cuentas que ingresan disponen del mismo menú, sin permisos diferenciados por rol.
