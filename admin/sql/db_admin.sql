--убрать подключения
SELECT pg_terminate_backend(pid) 
FROM pg_stat_activity 
WHERE datname = 'sensordb' 
  AND pid <> pg_backend_pid();


--типы данных представления
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND table_name = 'vw_dov_data_full'
ORDER BY ordinal_position;


--размер базы данных
SELECT pg_size_pretty(pg_database_size('sensordb')) as size;

--получить код представления. выберите любое
\d+ vw_dov_data_full
SELECT pg_get_viewdef('vw_dov_data_full');
SELECT view_definition FROM information_schema.views WHERE table_name = 'vw_dov_data_full';

создание резеврной копии sql 
ps
& "C:\Program Files\PostgreSQL\18\bin\pg_dump" -U postgres -h localhost -p 5432 -d sensordb -f "C:\temp\sensordb_new.sql"

& "C:\Program Files\PostgreSQL\18\bin\pg_dump" -U username -h localhost -p 5432 --schema-only -f "C:\temp\structure_backup.sql" sensordb