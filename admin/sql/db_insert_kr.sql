-- 1. Заполнение типов датчиков
INSERT INTO public."SensorType" 
    ("SensorTypeName", "Description", "CreatedAt")
VALUES
    ('DSPD',  'Датчик состояния дорожного полотна', NOW()),
    ('IWS',   'IWS', NOW()),
    ('DOV',   'Датчик оптической видимости', NOW()),    
    ('DUST',  'Датчик концентрации пыли', NOW()),
    ('MUEKS', 'Модуль управления электроснабжением', NOW())
ON CONFLICT ("SensorTypeName") DO NOTHING;

-- 2. Заполнение постов мониторинга (пример одного поста)
INSERT INTO public."MonitoringPost" 
    ("Name", "Description", "Longitude", "Latitude", "IsMobile", "IsActive", "CreatedAt", "UpdatedAt")
VALUES
    ('Новый пост 001', 'Стационарный пост 001 (основной)', 0, 0, false, true, NOW(), NOW())
ON CONFLICT DO NOTHING;

-- 3. Заполнение датчиков с указанием MonitoringPostId (предполагаем Id поста = 1)
INSERT INTO public."Sensor" 
    ("SensorTypeId", 
     "MonitoringPostId", 
     "SerialNumber", 
     "EndPointsName", 
     "Url", 
     "CheckIntervalSeconds", 
     "IsActive")
VALUES
    -- Основные датчики
    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DOV'),    1, 'DOV_01',   'DOV_01',    'http://192.168.1.10/json', 60, true),
    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'MUEKS'),    1, 'MUEKS_01',      'MUEKS_01',    'http://192.168.1.14/json', 60, true),
    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'IWS'), 1, 'IWS_01',   'IWS_01', 'http://192.168.1.16/json', 60, true),
    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DUST'), 1, 'DUST_01',   'DUST_01', 'http://46.23.183.51:8086/json', 60, true),    
    ((SELECT "Id" FROM "SensorType" WHERE "SensorTypeName" = 'DSPD'),    1, 'DSPD_01','DSPD_01','http://192.168.1.9/json', 60, true)

ON CONFLICT DO NOTHING;



-- 4. Стандартные конфигурации worker'ов
INSERT INTO public."WorkerConfiguration" 
    ("Key", "Value", "DataType", "Description", "IsActive")
VALUES 
    ('SaveResponseBody.Default', 'true', 'boolean', 'Сохранять ли тело ответа по умолчанию', true),
    ('Polling.MaxConcurrentTasks', '100', 'integer', 'Максимальное количество параллельных задач опроса', true),
    ('Polling.TimeoutSeconds', '30', 'integer', 'Таймаут HTTP запросов в секундах', true),
    ('Polling.RetryCount', '3', 'integer', 'Количество повторных попыток при ошибке', true),
    ('Polling.RetryDelayMs', '1000', 'integer', 'Задержка между повторными попытками в мс', true),
    ('Logging.FileLogLevel', 'Information', 'string', 'Уровень логирования в файл', true),
    ('Logging.DatabaseLogLevel', 'Warning', 'string', 'Уровень логирования в базу данных', true),
    ('Configuration.RefreshIntervalSeconds', '60', 'integer', 'Интервал обновления конфигурации', true)
ON CONFLICT ("Key") DO NOTHING;