DO $$
DECLARE
    post_cursor CURSOR FOR 
        SELECT "Id", "Name", "Description", "Longitude", "Latitude", "IsMobile", "IsActive", "CreatedAt", "UpdatedAt"
        FROM public."MonitoringPost";
    
    post_record RECORD;
BEGIN
    -- Открываем курсор
    OPEN post_cursor;
    
    LOOP
        -- Получаем следующую запись
        FETCH post_cursor INTO post_record;
        EXIT WHEN NOT FOUND;
        
        -- Пример обработки записи
        RAISE NOTICE 'Обработка поста: ID=%, Name=%, IsActive=%', 
            post_record."Id", 
            post_record."Name", 
            post_record."IsActive";
        
		INSERT INTO public."Sensor"     ("SensorTypeId",      "MonitoringPostId",     "SerialNumber",     "EndPointsName",     "Url",           "IsActive")
	    VALUES
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DOV'),    post_record."Id", 'DOV_local',      'dov_local',    'http://192.168.192.145:6009/api/Sensors/DOV',  true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DSPD'),   post_record."Id", 'DSPD_local_1',   'dspd_local_1', 'http://192.168.192.145:6009/api/Sensors/DSPD',  true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DUST'),    post_record."Id", 'DUST_001__local', 'dust_local', 'http://192.168.192.145:6009/api/Sensors/DUST',  true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'MUEKS'), post_record."Id", 'MUEKS_001_local',   'mueks_local', 'http://192.168.192.145:6009/api/Sensors/MUEKS',  true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'IWS'),    post_record."Id", 'IWS_001_local',   'iws_local',    'http://192.168.192.145:6009/api/Sensors/IWS',  true)
		ON CONFLICT DO NOTHING;
			    
    END LOOP;
    
    -- Закрываем курсор
    CLOSE post_cursor;
END $$;