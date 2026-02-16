-- 1. Заполнение типов датчиков
INSERT INTO public."SensorType" 
    ("SensorTypeName", "Description", "CreatedAt")
VALUES
    ('DSPD',   'Датчик состояния дорожного полотна', NOW()),
    ('IWS',    'IWS', NOW()),
    ('DOV',    'Датчик оптической видимости', NOW()),    
    ('DUST',    'Датчик концентрации пыли', NOW()),
    ('MUEKS', 'MUEKS', NOW())
ON CONFLICT ("SensorTypeName") DO NOTHING;

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