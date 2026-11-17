# 03 — CRUD relacional — módulo Autor

**Documento 03 — Autor. Operaciones autorizadas: Agregar, Buscar, Editar y Eliminar. Implementación pendiente de pruebas del estudiante.** No se ha ejecutado Agregar, Editar ni Eliminar contra la base de datos ni se ha modificado su esquema.

## Esquema confirmado

`dbo.Tbl_Autor`: `IDAutor int NOT NULL` (PK), `IDLibro int NOT NULL` (FK), `NombreAutor varchar(100) NOT NULL`, `Nacionalidad varchar(50) NULL`, `Biografia varchar(100) NOT NULL`, `FechaNacimiento datetime NULL`.

Relación: `Tbl_Autor.IDLibro` → `Tbl_Libros.IDLibro`. Para el selector, `IDLibro` es `ValueMember` y `Titulo` es `DisplayMember`.

## Alcance vigente

- SELECT de Autor con JOIN a libros y búsqueda por IDAutor, IDLibro, NombreAutor y Nacionalidad usando parámetros.
- Carga del catálogo `IDLibro`/`Titulo` en ComboBox con `ValueMember` y `DisplayMember`.
- La grilla pagina de 10 en 10 desde Presentación; los controles se ocultan si hay una sola página.
- Selector de libro agregado en la posición del control anterior.
- `IDAutor` fue confirmado como identity con una consulta de metadatos de solo lectura; el alta omite ese campo.
- Búsqueda sobre `IDAutor`, `IDLibro`, `NombreAutor` y `Nacionalidad`.
- Agregar, editar y eliminar usan parámetros en Datos; Negocio valida campos y Presentación procesa eventos.
- Totales globales se vuelven a consultar después de cada cambio y no se calculan desde la página mostrada.
- Paginación de 10 registros exclusivamente en Presentación; los controles de página se ocultan cuando solo existe una página.

El código actual no se ha compilado ni probado; corresponde al estudiante hacerlo y confirmar el resultado. No implementar Login/roles ni otras funciones sin autorización específica.
