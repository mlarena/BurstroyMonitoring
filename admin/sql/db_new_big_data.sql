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
        
		INSERT INTO public."Sensor"     ("SensorTypeId",      "MonitoringPostId",     "SerialNumber",     "EndPointsName",     "Url",      "CheckIntervalSeconds",     "IsActive")
		VALUES
		    -- Основные датчики
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DSPD'),   post_record."Id", 'DSPD_001', 'dspd', 'http://85.26.216.15:8082/json', 300, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DSPD'),   post_record."Id", 'DSPD_002', 'dspd_2', 'http://85.26.216.15:8082/json', 300, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DSPD'),   post_record."Id", 'DSPD_M', 'dspd_m', 'http://213.87.15.137:8088/json', 30, true),    
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DOV'),    post_record."Id", 'DOV_001', 'dov', 'http://85.26.216.15:8085/json', 60, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DOV'),    post_record."Id", 'DOV_002', 'dov_2', 'http://85.26.216.15:8085/json', 90, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'MUEKS'), post_record."Id", 'MUEKS_001', 'mueks', 'http://85.26.216.15:8083/json', 60, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'MUEKS'), post_record."Id", 'MUEKS_002', 'mueks_2', 'http://85.26.216.15:8083/json', 180, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'IWS'),    post_record."Id", 'IWS_001', 'iws', 'http://85.26.216.15:8084/json', 60, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'IWS'),    post_record."Id", 'IWS_002', 'iws_2', 'http://85.26.216.15:8084/json', 300, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DUST'),    post_record."Id", 'DUST_001', 'dust_2', 'http://46.23.183.51:8086/json', 300, true),
		    -- Локальные датчики
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DSPD'),   post_record."Id", 'DSPD_local_001', 'dspd_local_1', 'http://192.168.3.29/json', 120, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DSPD'),   post_record."Id", 'DSPD_local_002', 'dspd_local_2', 'http://192.168.3.26/json', 120, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DSPD'),   post_record."Id", 'DSPD_local_3',   'dspd_local_3', 'http://192.168.3.28/json', 120, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DOV'),    post_record."Id", 'DOV_local',      'dov_local',    'http://192.168.3.43/json', 90, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'MUEKS'), post_record."Id", 'MUEKS_local',   'mueks_local', 'http://192.168.3.42/json', 180, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'IWS'),    post_record."Id", 'IWS_00_local',   'iws_local',    'http://192.168.3.45/json', 60, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'IWS'),    post_record."Id", 'IWS_00_local_new','iws_local_new','http://192.168.3.45/json', 60, true)
		ON CONFLICT DO NOTHING;
		    
    END LOOP;
    
    -- Закрываем курсор
    CLOSE post_cursor;
END $$;