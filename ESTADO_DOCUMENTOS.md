# Estado de documentos — Programación II 2026

Documento activo: **Documento 06 — cierre pendiente de confirmación unánime de los tres integrantes**. El Documento 04 — Maestro-detalle (Devoluciones y DetalleDevolucion) quedó concluido por el estudiante el 2026-10-17 y puede reabrirse cuando lo solicite. El estudiante corrigió el orden: Documento 02 es CRUD simple y Documento 03 es CRUD relacional. La guía de conexión SQL Server se conserva como referencia en el Skill 05.

| Documento | Estado | Nota verificada |
|---|---|---|
| 01 Inspección inicial | **COMPLETADO y VALIDADO** | Estructura revisada; solución existente inspeccionada; conexión, esquema, respaldo, capas, referencias y formularios resumidos para revisión. El estudiante envió la frase explícita de validación. |
| 02 CRUD simple | Pendiente — no iniciado | Guía: `skills/02_CRUD_Simple.md`. Requiere autorización propia antes de iniciarse. |
| 03 CRUD relacional — Autor y Categorías | **IMPLEMENTADO Y VALIDADO POR EL ESTUDIANTE** | Para Autor, el estudiante confirmó que ejecutó y compiló la versión y que está bien. Para Categorías, confirmó que ejecutó y compiló los cambios y que las pruebas fueron exitosas. Categorías incluye CRUD, búsqueda por cuatro campos, totales globales y paginación de 10 filas en Presentación. Guía: `skills/03_CRUD_Relacional.md`. |
| 04 Maestro-detalle | **CONCLUIDO POR EL ESTUDIANTE (2026-10-17)** | Revisión de capas, consultas, transacciones, parámetros, controles y eventos completada. Rebuild Debug / Any CPU exitoso con MSBuild 17.14.40, sin errores ni advertencias. El estudiante confirmó que probó el módulo y lo considera bien; indicó que también probó el ajuste de bloqueo de escritura directa en campos y dio el documento por concluido. La comprobación SQL de solo lectura encontró cero detalles huérfanos en ese momento. No existe FK física entre `Tbl_DetalleDevolucion.IDDevoluciones` y `Tbl_Devoluciones.IDDevoluciones`; el esquema no se modificó. El documento queda cerrado hasta solicitud del estudiante. Guía: `skills/04_Maestro_Detalle.md`. |
| 05 Conexión SQL Server | Completado; disponible como referencia | La solución compiló con MSBuild de Visual Studio (0 errores, 0 advertencias). La prueba de `Datos.ConexionDatos.ProbarConexion` con un arnés temporal compilado con Roslyn de Visual Studio y .NET Framework tuvo resultado exitoso. No se ejecutó F5 desde la interfaz del IDE. La conexión previa con `sqlcmd`/ODBC 18 falló por cifrado. Guía: `skills/05_Conexion_SQLServer.md`. |
| 06 Git colaborativo | **CERRADO (2026-10-22)** | GitHub confirmó que el repositorio es privado; se verificaron ramas remotas `Kevin`, `Christian`, `Emerson` y `main`; el historial de `main` contiene los merges de PR #1, #2 y #3. El estudiante confirmó que los tres integrantes dieron su conformidad para los requisitos del paso 5.10. El registro del cierre se integró en `main` mediante el PR #4 (merge `653a250cfd488c1da23edaba0928a3b8ffbb6e23`). |
| 07 Login y roles | Pendiente — bloqueado | Se trabaja al final; requiere habilitación específica. |
| 08 Panel de navegación | Pendiente — bloqueado | Requiere habilitación específica. |
| 09 Dashboard | Pendiente — bloqueado | Requiere habilitación específica. |
| 10 Revisión del proyecto | **Revisión parcial autorizada — módulo Devoluciones** | Revisión del módulo 04 completada el 2026-10-17 mediante `skills/10_Revision_Proyecto.md`; una revisión integral de todo el proyecto sigue pendiente. El archivo citado en la solicitud `skills/05_Maestro_Detalle.md` no existe en el árbol actual; el documento 05 existente es `skills/05_Conexion_SQLServer.md`. |
| 11 Publicación | Pendiente | Requiere autorización posterior. |

## Hechos comprobados para la transición

## Verificación del cierre del Documento 06 — 2026-10-22

- `git ls-remote --heads origin` consultó GitHub correctamente y devolvió las ramas publicadas `Kevin`, `Christian`, `Emerson` y `main`. La rama remota `main` apunta a `4b92ed3`. La API de GitHub confirmó que el repositorio es privado y que `main` es la rama predeterminada.
- El historial local actualizado de `main` muestra commits de integración identificados como Pull Requests #1 (Kevin), #2 (Christian) y #3 (Emerson).
- Se publicó `docs/cierre-documento-06` y se integró en `main` mediante el PR #4 (merge `653a250cfd488c1da23edaba0928a3b8ffbb6e23`).
- El estudiante confirmó que los tres integrantes dieron su conformidad con los requisitos del paso 5.10; el Documento 06 queda cerrado.

- SQL Server está en el contenedor Docker `sql_prog2b`; el host publica TCP 1434 hacia TCP 1433 del contenedor. La base es `libreriaProyecto`. No se guarda la contraseña en este documento.
- Se verificaron en modo solo lectura 14 tablas, tipos, nulabilidad, claves primarias y 15 claves foráneas. El esquema completo se documenta en `database/CONEXION_DATABASE.md`; no se consultaron filas ni se alteraron tablas.
- El respaldo `database/libreriaProyecto_20261013.bak` existe fuera del contenedor, mide 4,968,448 bytes y pasó `RESTORE VERIFYONLY`. SHA-256: `18a59636d0a2131b2de668e2517ef8051df39ac48dd36ccb31b1034ba75df202`.
- La solución tiene cuatro proyectos .NET Framework 4.8: Entidad, Datos, Negocio y Presentación. Referencias actuales: Datos → Entidad; Negocio → Datos y Entidad; Presentación → Negocio y Entidad.
- `Presentacion/App.config` contiene la cadena de conexión. `Datos/ConexionDatos.cs` lee esa configuración, abre y cierra conexión con `try-catch`, y no implementa operaciones CRUD.
- Formularios inspeccionados: AutoresForms, Categorias, DetalleDevolucionForms y DevolucionesForms, con sus archivos `.cs`, `.Designer.cs` y `.resx`. Los identificadores de controles asociados a datos se hicieron más descriptivos; se conservaron estilos y posiciones. Los CRUD implementados corresponden a AutoresForms y Categorias. El estudiante confirmó que compiló y probó ambas versiones; informó que las pruebas de Categorías fueron exitosas.
- La conexión con ODBC 18 desde Windows había fallado durante el cifrado; la prueba posterior con `System.Data.SqlClient` sí conectó. Mantener diferenciados ambos resultados.

## Revisión del módulo Devoluciones — 2026-10-17

- La conexión real con SQL Server `libreriaProyecto` abrió mediante la cadena configurada por la aplicación. La consulta de metadatos fue de solo lectura. Confirmó `Tbl_Devoluciones.IDDevoluciones int NOT NULL IDENTITY` como PK; `Tbl_DetalleDevolucion.IDDetalleDaevolucion int NOT NULL IDENTITY` como PK; y la FK `Tbl_Devoluciones.IDPrestamo` → `Tbl_Prestamo.IDPrestamo`. También confirmó `Tbl_Pagos.IDDevoluciones` → `Tbl_Devoluciones.IDDevoluciones` y que no hay una FK desde `Tbl_DetalleDevolucion.IDDevoluciones` hacia el encabezado. No se consultaron datos de filas ni se modificó SQL Server.
- Se revisaron las cuatro capas y los controles/eventos ya existentes en `DevolucionesForms` y `DetalleDevolucionForms`. El código enlaza Buscar, Editar, Eliminar, Limpiar y Cancelar en sus modos correspondientes. Las sentencias inspeccionadas usan parámetros SQL; el alta encabezado-detalle y el borrado encabezado-detalles contienen `Commit`/`Rollback`. La paginación de diez registros y el recorte de página están en Presentación; los totales se consultan aparte. Es una comprobación estática; no confirma la ejecución funcional de cada botón.
- `LibreriaProyectoProgra2.sln` se reconstruyó en Debug / Any CPU con MSBuild 17.14.40: Entidad, Datos, Negocio y Presentación finalizaron sin errores ni advertencias. No se abrió la interfaz ni se probaron operaciones que cambien registros. La prueba funcional del estudiante sigue pendiente.
- En el árbol actual no existe `skills/05_Maestro_Detalle.md`; sí existen `skills/04_Maestro_Detalle.md`, `skills/05_Conexion_SQLServer.md` y `skills/10_Revision_Proyecto.md`. La revisión usó la guía 04 como especificación vigente y la guía 10 solicitada. No se ejecutó Git/GitHub.
- Después de la revisión, el estudiante confirmó que ejecutó y probó el módulo y que lo ve bien. La comprobación estática de este entorno no fue una ejecución independiente de los botones. El conteo en solo lectura devolvió cero detalles sin encabezado en ese momento. La ausencia de FK sigue siendo una brecha: una inserción externa a la aplicación podría crear huérfanos; no se solicitó ni realizó DDL.

## Alcance habilitado

El alcance vigente del Documento 03 incluye Autor y Categorías. Autor tiene las operaciones autorizadas Agregar, Buscar, Editar y Eliminar, catálogo relacional de libros, filtro por IDAutor/IDLibro/NombreAutor/Nacionalidad, totales globales y paginación de 10 filas en Presentación; el estudiante confirmó que compiló y probó esa versión y que está bien. Para Categorías se implementaron Agregar, Buscar, Editar y Eliminar, catálogo por IDLibro, filtro por IDCategoria/IDLibro/Categoria/Descripcion, totales globales y paginación de 10 filas en Presentación; el estudiante confirmó que compiló, ejecutó y obtuvo pruebas exitosas. Confirmó que IDCategoria es IDENTITY. Documento 04 quedó concluido por el estudiante el 2026-10-17: consulta, alta transaccional y operaciones Buscar/Editar/Eliminar/Limpiar/Cancelar en Devoluciones y DetalleDevolucion; borrado de encabezado y detalles atómico; operaciones de detalle acotadas al encabezado seleccionado; paginación de 10 filas solo en Presentación y totales globales independientes; tabla física singular `Tbl_DetalleDevolucion`. Compilación exitosa y prueba general confirmada por el estudiante. No se alteró la base. No reabrir Documento 04 ni iniciar otro documento hasta que el estudiante lo solicite. Mantener la arquitectura Entidad/Datos/Negocio/Presentación y preferir `cmd.exe` para comandos de Windows cuando esté disponible.

## Verificación del cierre Git — 2026-10-22

- Actualización remota posterior: el registro recibido desde `origin/main` indica que el repositorio es privado y que los tres integrantes confirmaron los requisitos colectivos; el cierre se integró mediante el PR #4. Este dato reemplaza el estado anterior de “falta acuerdo colectivo”.

- El commit de Kevin `862c464` (`CRUD asignados - Kevin`) se publicó en `origin/Kevin`; el PR #1 está integrado en `main` mediante `97a75fe`, verificado como ancestro de `main`.
- La copia clonada se actualizó desde `origin/main` hasta `9657601`. La solución se había reconstruido completa con MSBuild 18.7.1 después de integrar el CRUD de Kevin; esto comprueba compilación, no prueba funcional.
- No consta evidencia de pruebas funcionales de CRUD por los tres estudiantes. Tampoco se verificó la privacidad del repositorio ni se recibieron las tres confirmaciones sobre privacidad y conservación de carpetas originales. El Documento 06 sigue sin declararse finalizado hasta recibir acuerdo explícito de los tres.
- Se creó localmente `docs/cierre-documento-06-git` desde `main` para esta actualización; requiere push y PR para compartirla.
- La secuencia del usuario para lo posterior (Documento 06 Login, 07 Panel y 08 Dashboard) difiere de la tabla actual (06 Git, 07 Login, 08 Panel y 09 Dashboard). Aclarar numeración/alcance y autorización antes de programar módulos; trabajar en rama, no directamente en `main`.
