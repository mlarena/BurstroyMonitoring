-- 1. Таблица типов датчиков
CREATE TABLE IF NOT EXISTS public."SensorType" (
    "Id"          SERIAL PRIMARY KEY,
    "SensorTypeName" VARCHAR(20) NOT NULL UNIQUE,
    "Description" TEXT NOT NULL,
    "CreatedAt"   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. Таблица постов мониторинга
CREATE TABLE IF NOT EXISTS public."MonitoringPost" (
    "Id"          SERIAL PRIMARY KEY,
    "Name"        VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "Longitude"   DOUBLE PRECISION,
    "Latitude"    DOUBLE PRECISION,
    "IsMobile"    BOOLEAN NOT NULL DEFAULT FALSE,
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt"   TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_monitoringpost_coordinates_valid CHECK (
        (("Longitude" IS NULL AND "Latitude" IS NULL) OR
         ("Longitude" BETWEEN -180 AND 180 AND "Latitude" BETWEEN -90 AND 90))
    )
);

-- Индексы для MonitoringPost
CREATE INDEX IF NOT EXISTS idx_monitoringpost_name        ON public."MonitoringPost" ("Name");
CREATE INDEX IF NOT EXISTS idx_monitoringpost_active      ON public."MonitoringPost" ("IsActive");
CREATE INDEX IF NOT EXISTS idx_monitoringpost_mobile      ON public."MonitoringPost" ("IsMobile");
CREATE INDEX IF NOT EXISTS idx_monitoringpost_coordinates ON public."MonitoringPost" ("Longitude", "Latitude");
CREATE INDEX IF NOT EXISTS idx_monitoringpost_created_at  ON public."MonitoringPost" ("CreatedAt");

-- 3. Основная таблица датчиков (с добавленной связью MonitoringPostId)
CREATE TABLE IF NOT EXISTS public."Sensor" (
    "Id"                SERIAL PRIMARY KEY,
    "SensorTypeId"      INTEGER NOT NULL REFERENCES public."SensorType"("Id") ON DELETE RESTRICT,
    "MonitoringPostId"  INTEGER,  -- Связь с постом мониторинга
    "Longitude"         DOUBLE PRECISION,
    "Latitude"          DOUBLE PRECISION,
    "SerialNumber"      VARCHAR(64) NOT NULL,
    "EndPointsName"     VARCHAR(255) NOT NULL,
    "Url"               TEXT NOT NULL,
    "CheckIntervalSeconds" INTEGER NOT NULL DEFAULT 300,
    "LastActivityUTC"   TIMESTAMPTZ,
    "CreatedAt"         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "IsActive"          BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT chk_sensor_coordinates_valid CHECK (
        (("Longitude" IS NULL AND "Latitude" IS NULL) OR
         ("Longitude" BETWEEN -180 AND 180 AND "Latitude" BETWEEN -90 AND 90))
    ),

    CONSTRAINT "Sensor_MonitoringPostId_fkey"
        FOREIGN KEY ("MonitoringPostId")
        REFERENCES public."MonitoringPost"("Id")
        ON DELETE SET NULL
);

-- Индексы для Sensor
CREATE INDEX IF NOT EXISTS idx_sensor_active_type    ON public."Sensor" ("IsActive", "SensorTypeId");
CREATE INDEX IF NOT EXISTS idx_sensor_serial         ON public."Sensor" ("SerialNumber");
CREATE INDEX IF NOT EXISTS idx_sensor_check_interval ON public."Sensor" ("CheckIntervalSeconds");
CREATE INDEX IF NOT EXISTS idx_sensor_last_activity  ON public."Sensor" ("LastActivityUTC");
CREATE INDEX IF NOT EXISTS idx_sensor_monitoringpost ON public."Sensor" ("MonitoringPostId");

-- 4. Общие таблицы ошибок и результатов опроса
CREATE TABLE IF NOT EXISTS public."SensorError" (
    "Id"              SERIAL PRIMARY KEY,
    "SensorId"        INTEGER NOT NULL REFERENCES public."Sensor"("Id") ON DELETE RESTRICT,
    "ErrorLevel"      VARCHAR(20) NOT NULL,
    "ErrorMessage"    TEXT NOT NULL,
    "StackTrace"      TEXT,
    "ErrorSource"     VARCHAR(255),
    "ExceptionType"   VARCHAR(255),
    "CreatedAt"       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "AdditionalData"  JSONB
);

CREATE TABLE IF NOT EXISTS public."SensorResults" (
    "Id"            SERIAL PRIMARY KEY,
    "SensorId"      INTEGER NOT NULL REFERENCES public."Sensor"("Id") ON DELETE RESTRICT,
    "CheckedAt"     TIMESTAMPTZ NOT NULL,
    "StatusCode"    INTEGER NOT NULL,
    "ResponseBody"  JSONB,
    "ResponseTimeMs" BIGINT NOT NULL,
    "IsSuccess"     BOOLEAN NOT NULL
);

-- Индексы для SensorError и SensorResults
CREATE INDEX IF NOT EXISTS idx_sensor_error_sensor   ON public."SensorError" ("SensorId");
CREATE INDEX IF NOT EXISTS idx_sensor_error_created  ON public."SensorError" ("CreatedAt");
CREATE INDEX IF NOT EXISTS idx_sensor_error_level    ON public."SensorError" ("ErrorLevel");

CREATE INDEX IF NOT EXISTS idx_sensor_results_sensor  ON public."SensorResults" ("SensorId");

CREATE INDEX IF NOT EXISTS idx_sensor_results_checked ON public."SensorResults" ("CheckedAt");

CREATE INDEX IF NOT EXISTS idx_sensor_results_success ON public."SensorResults" ("IsSuccess");

-- 5. Специализированные таблицы данных по типам датчиков
CREATE TABLE IF NOT EXISTS public."DSPDData" (
    "Id"                  SERIAL PRIMARY KEY,
    "SensorId"            INTEGER NOT NULL REFERENCES public."Sensor"("Id") ON DELETE RESTRICT,
    "ReceivedAt"          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "DataTimestamp"       TIMESTAMPTZ NOT NULL,
    "Grip"                DECIMAL(5,2),
    "Shake"               DECIMAL(5,2),
    "UPower"              DECIMAL(5,2),
    "TemperatureCase"     DECIMAL(5,2),
    "TemperatureRoad"     DECIMAL(5,2),
    "HeightH2O"           DECIMAL(5,2),
    "HeightIce"           DECIMAL(5,2),
    "HeightSnow"          DECIMAL(5,2),
    "PercentICE"          DECIMAL(5,2),
    "PercentPGM"          DECIMAL(5,2),
    "RoadStatus"          INTEGER,
    "AngleToRoad"         DECIMAL(5,2),
    "TemperatureFreezePGM" DECIMAL(5,2),
    "NeedCalibration"     INTEGER,
    "GPSLatitude"         DECIMAL(10,6),
    "GPSLongitude"        DECIMAL(10,6),
    "DistanceToSurface" DECIMAL(12,2) NULL,
    "IsGpsValid"          BOOLEAN DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS public."IWSData" (
    "Id"                  SERIAL PRIMARY KEY,
    "SensorId"            INTEGER NOT NULL REFERENCES public."Sensor"("Id") ON DELETE RESTRICT,
    "ReceivedAt"          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "DataTimestamp"       TIMESTAMPTZ NOT NULL,
    "EnvTemperature"      DECIMAL(5,2),
    "Humidity"            DECIMAL(5,2),
    "DewPoint"            DECIMAL(5,2),
    "PressureHPa"         DECIMAL(7,2),
    "PressureQNHHPa"      DECIMAL(7,2),
    "PressureMmHg"        DECIMAL(7,2),
    "WindSpeed"           DECIMAL(5,2),
    "WindDirection"       DECIMAL(6,2),
    "WindVSound"          DECIMAL(6,2),
    "PrecipitationType"   INTEGER,
    "PrecipitationIntensity" DECIMAL(5,2),
    "PrecipitationQuantity"  DECIMAL(5,2),
    "PrecipitationElapsed"   INTEGER,
    "PrecipitationPeriod"    INTEGER,
    "CO2Level" DECIMAL(6,2),
    "SupplyVoltage"       DECIMAL(5,1),
    "Latitude"            DECIMAL(10,6),
    "Longitude"           DECIMAL(10,6),
    "Altitude"            DECIMAL(7,2),
    "KSP"                 INTEGER,
    "GPSSpeed"            DECIMAL(5,1),
    "AccelerationStDev"   DECIMAL(5,2),
    "Roll"                DECIMAL(5,1),
    "Pitch"               DECIMAL(5,1),
    "WeAreFine"           INTEGER
);

CREATE TABLE IF NOT EXISTS public."MUEKSData" (
    "Id"            SERIAL PRIMARY KEY,
    "SensorId"      INTEGER NOT NULL REFERENCES public."Sensor"("Id") ON DELETE RESTRICT,
    "ReceivedAt"    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "DataTimestamp" TIMESTAMPTZ NOT NULL,
    "TemperatureBox" DECIMAL(5,2),
    "UPowerIn12B"   DECIMAL(5,2),
    "UOut12B"       DECIMAL(5,2),
    "IOut12B"       DECIMAL(5,2),
    "IOut48B"       DECIMAL(5,2),
    "UAkb"          DECIMAL(5,2),
    "IAkb"          DECIMAL(5,2),
    "Sens220B"      INTEGER,
    "WhAkb"         DECIMAL(10,2),
    "VisibleRange"  DECIMAL(10,2),
    "DoorStatus"    INTEGER,
    "TdsH"          VARCHAR(50),
    "TdsTds"        VARCHAR(50),
    "TkosaT1"       VARCHAR(50),
    "TkosaT3"       VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS public."DOVData" (
    "Id"            SERIAL PRIMARY KEY,
    "SensorId"      INTEGER NOT NULL REFERENCES public."Sensor"("Id") ON DELETE RESTRICT,
    "ReceivedAt"    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "DataTimestamp" TIMESTAMPTZ NOT NULL,
    "VisibleRange"  DECIMAL(10,2),
    "BrightFlag"    INTEGER
);

CREATE TABLE IF NOT EXISTS public."DustData" (
    "Id"                   SERIAL PRIMARY KEY,
    "SensorId"             INTEGER NOT NULL REFERENCES public."Sensor"("Id") ON DELETE RESTRICT,
    "ReceivedAt"           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "DataTimestamp"        TIMESTAMPTZ NOT NULL,
    
    -- Значения размер частиц микрограмм на куб метр
    "PM10Act"             DECIMAL(10,2) NULL,
    "PM25Act"            DECIMAL(10,2) NULL,
    "PM1Act"             DECIMAL(10,2) NULL,
    
    -- Средние значения  частиц микрограмм на куб метр
    "PM10AWG"            DECIMAL(10,2) NULL,
    "PM25AWG"            DECIMAL(10,2) NULL,
    "PM1AWG"            DECIMAL(10,2) NULL,
    
    -- Данные датчиков
    "FlowProbe"           DECIMAL(10,2) NULL,
    "TemperatureProbe"    DECIMAL(10,2) NULL,
    "HumidityProbe"       DECIMAL(10,2) NULL,
    
    -- Статус и питание
    "LaserStatus"         INTEGER NULL,
    "SupplyVoltage"       DECIMAL(10,2) NULL
);

-- Создаем индекс для SensorId для ускорения JOIN операций
CREATE INDEX IF NOT EXISTS "idx_DustData_SensorId" ON public."DustData" ("SensorId");

-- Создаем индекс для DataTimestamp для ускорения временных запросов
CREATE INDEX IF NOT EXISTS "idx_DustData_DataTimestamp" ON public."DustData" ("DataTimestamp");

-- Индексы для специализированных таблиц данных
CREATE INDEX IF NOT EXISTS idx_dspddata_sensor_timestamp ON public."DSPDData" ("SensorId", "DataTimestamp");
CREATE INDEX IF NOT EXISTS idx_dspddata_timestamp       ON public."DSPDData" ("DataTimestamp");
CREATE INDEX IF NOT EXISTS idx_dspddata_roadstatus      ON public."DSPDData" ("RoadStatus");
CREATE INDEX IF NOT EXISTS idx_dspddata_grip            ON public."DSPDData" ("Grip");

CREATE INDEX IF NOT EXISTS idx_iwsdata_sensor_timestamp ON public."IWSData" ("SensorId", "DataTimestamp");
CREATE INDEX IF NOT EXISTS idx_iwsdata_timestamp       ON public."IWSData" ("DataTimestamp");
CREATE INDEX IF NOT EXISTS idx_iwsdata_weather         ON public."IWSData" ("EnvTemperature", "Humidity", "WindSpeed");

CREATE INDEX IF NOT EXISTS idx_mueksdata_sensor_timestamp ON public."MUEKSData" ("SensorId", "DataTimestamp");
CREATE INDEX IF NOT EXISTS idx_mueksdata_timestamp       ON public."MUEKSData" ("DataTimestamp");
CREATE INDEX IF NOT EXISTS idx_mueksdata_visibility      ON public."MUEKSData" ("VisibleRange");

CREATE INDEX IF NOT EXISTS idx_dovdata_sensor_timestamp ON public."DOVData" ("SensorId", "DataTimestamp");
CREATE INDEX IF NOT EXISTS idx_dovdata_timestamp       ON public."DOVData" ("DataTimestamp");
CREATE INDEX IF NOT EXISTS idx_dovdata_visibility      ON public."DOVData" ("VisibleRange");

CREATE TABLE IF NOT EXISTS public."WorkerConfiguration" (
    "Id"           SERIAL PRIMARY KEY,
    "Key"          VARCHAR(100) NOT NULL UNIQUE,
    "Value"        TEXT NOT NULL,
    "DataType"     VARCHAR(50) NOT NULL CHECK ("DataType" IN ('boolean', 'integer', 'string', 'decimal', 'json')),
    "Description"  TEXT,
    "LastModified" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "ModifiedBy"   VARCHAR(100),
    "IsActive"     BOOLEAN NOT NULL DEFAULT TRUE
);



