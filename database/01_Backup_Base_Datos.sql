-- Respaldo completo de la base verificada libreriaProyecto.
-- La ruta existe en el contenedor SQL Server y es escribible por el usuario mssql.
-- COPY_ONLY evita alterar la secuencia normal de respaldos.
BACKUP DATABASE [libreriaProyecto]
TO DISK = N'/var/opt/mssql/data/libreriaProyecto_20261013.bak'
WITH COPY_ONLY, CHECKSUM, STATS = 10;
GO
