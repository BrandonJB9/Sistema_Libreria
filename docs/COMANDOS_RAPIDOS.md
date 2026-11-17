# Comandos rápidos

## Windows

- Preferir `cmd.exe` para ejecutar comandos en Windows. Usar PowerShell solo si CMD no está disponible.
- `cd` — muestra la ruta actual.
- `dir /a` — lista el contenido, incluidos elementos ocultos.
- `tree /a` — muestra el árbol de directorios con caracteres ASCII.
- `mkdir nombre` — crea una carpeta.

## Reglas del proyecto

- Tecnología prevista: C# con .NET Framework, Windows Forms y SQL Server.
- Capas: Entidad, Datos, Negocio y Presentación.
- Usar POO básica, consultas parametrizadas, `try-catch` y comentarios didácticos.
- Conservar los diseños existentes.
- No avanzar a SQL Server hasta que el estudiante confirme el árbol real.
- Login y roles al final; Git/GitHub después de los CRUD y antes de Login.

## Rama y Pull Request de Kevin

Resultados comprobados el 2026-10-22: rama `Kevin`, commit `862c464` (`CRUD asignados - Kevin`) publicado en `origin/Kevin`; PR #1 integrado en `main` mediante el merge `97a75fe`. Para los cambios documentales del cierre, se creó `docs/cierre-documento-06-git` desde `main`.

```cmd
git switch main
git pull origin main
git switch -c docs/cierre-documento-06-git
```

Revisar archivos nuevos (untracked) y modificados. Agregar solo las rutas revisadas; no usar `git add .`:

```cmd
git status --short
git add MEMORY.md ESTADO_DOCUMENTOS.md docs/COMANDOS_RAPIDOS.md
git diff --cached --stat
git diff --cached
git status
git commit -m "Documenta cierre de Git colaborativo"
git push -u origin docs/cierre-documento-06-git
```

Después del push, abrir un Pull Request de `docs/cierre-documento-06-git` hacia `main`. Un rebuild exitoso no demuestra que los CRUD se hayan probado funcionalmente.

## Cierre del Documento 06 — verificación 5.10

Desde la copia clonada, revisar las ramas publicadas y los merges registrados:

```cmd
git branch -r
git log --oneline --merges origin/main -n 10
git status --short --branch
```

El repositorio se verificó como privado. El historial verificado el 2026-10-22 registra los PR #1, #2, #3 y #4 integrados en `main`; el estudiante confirmó que los tres integrantes dieron su conformidad con los requisitos del paso 5.10. El Documento 06 se registra como cerrado.
## Git colaborativo y cierre documental

Ejecutar estos comandos en la copia clonada. Revisar los archivos explícitos antes de confirmar; no usar git add .

    git switch main
    git pull --ff-only origin main
    git switch -c docs/cierre-documento-06-2026-10-22
    git status --short
    git add -- MEMORY.md ESTADO_DOCUMENTOS.md docs/COMANDOS_RAPIDOS.md
    git diff --cached --stat
    git diff --cached
    git commit -m "Actualizar evidencia de Git colaborativo"
    git push -u origin docs/cierre-documento-06-2026-10-22

Después, crear el Pull Request de la rama documental hacia main y esperar la revisión del grupo. Documento 06 solo se declara finalizado cuando los tres integrantes confirmen privacidad, ramas publicadas, PR integrados y CRUD probados. No programar directamente en main.
