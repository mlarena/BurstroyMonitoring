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
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DSPD'),   post_record."Id", 'DSPD_M', 'состояние дорожного полотна', 'http://localhost:6009/api/Sensors/DSPD', 60, true),    
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DOV'),    post_record."Id", 'DOV_001', 'видимость', 'http://localhost:6009/api/Sensors/DOV', 60, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'MUEKS'), post_record."Id", 'MUEKS_001', 'управление энергоснабжением', 'http://localhost:6009/api/Sensors/MUEKS', 60, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'IWS'),    post_record."Id", 'IWS_001', 'параметроы атмосферы', 'http://localhost:6009/api/Sensors/IWS', 60, true),
		    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DUST'),    post_record."Id", 'DUST_001', 'концентрация пыли', 'http://localhost:6009/api/Sensors/DUST', 60, true)
		ON CONFLICT DO NOTHING;
		    
    END LOOP;
    
    -- Закрываем курсор
    CLOSE post_cursor;
END $$;