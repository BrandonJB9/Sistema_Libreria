# 01 — Inspección inicial

## Objetivo

Conocer y registrar el estado real del directorio antes de preparar el proyecto. No inferir archivos, herramientas, diseños ni configuraciones que no se hayan comprobado.

## Reglas

- En Windows, preferir `cmd.exe` para ejecutar comandos; recurrir a PowerShell solo si CMD no está disponible.
- Revisar la ruta actual y listar archivos, incluidos los ocultos.
- Registrar los resultados observados y distinguirlos de las propuestas.
- Conservar diseños y archivos existentes. No sobrescribir ni borrar elementos ajenos al paso autorizado.
- No crear soluciones ni formularios en la inspección inicial.
- Preguntar al estudiante al terminar cada paso.

## Procedimiento

1. Consultar la ruta actual con `cd`.
2. Listar el contenido con `dir /a`.
3. Resumir qué existe y qué no se pudo verificar.
4. Proponer el árbol inicial y esperar autorización antes de crearlo.
5. Tras crear el árbol, pedir al estudiante que confirme que corresponde al árbol real antes de avanzar a SQL Server.

## Registro comprobado de este paso

- La ruta inspeccionada fue `C:\2026_Progra2_ProyectoFinal`.
- En la inspección inicial, la carpeta no contenía archivos ni subcarpetas del proyecto.
- El estudiante autorizó crear la estructura inicial propuesta.
- La estructura solicitada se creó; queda pendiente la confirmación del estudiante sobre el árbol real.
- No se preparó ni ejecutó una conexión a SQL Server.
