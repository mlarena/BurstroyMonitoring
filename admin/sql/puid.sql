-- SQL для создания таблиц и связей
CREATE TABLE IF NOT EXISTS "Puid" (
    "Id" SERIAL PRIMARY KEY,
    "SensorType" VARCHAR(64) DEFAULT 'PUID' NOT NULL,
    "MonitoringPostId" INT NULL,
    "Longitude" FLOAT8 NULL,
    "Latitude" FLOAT8 NULL,
    "SerialNumber" VARCHAR(64) NOT NULL,
    "EndPointsName" VARCHAR(255) NOT NULL,
    "IntervalSeconds" int4 DEFAULT 60 NOT NULL,
    "Url" TEXT NOT NULL,
    "LastActivityUTC" TIMESTAMPTZ NULL,
    "CreatedAt" TIMESTAMPTZ DEFAULT NOW() NOT NULL,
    "IsActive" BOOL DEFAULT TRUE NOT NULL
);

CREATE TABLE IF NOT EXISTS "PuidData" (
    "Id" SERIAL PRIMARY KEY,
    "PuidId" INT NOT NULL,
    "MessageId" UUID NOT NULL,
    "SensorId" UUID NOT NULL,
    "SensorName" TEXT,
    "Direction" INT,
    "Lane" INT NOT NULL,
    "Volume" INT,
    "Class0" INT,
    "Class1" INT,
    "Class2" INT,
    "Class3" INT,
    "Class4" INT,
    "Class5" INT,
    "GapAvg" FLOAT8,
    "GapSum" FLOAT8,
    "SpeedAvg" FLOAT8,
    "HeadwayAvg" FLOAT8,
    "HeadwaySum" FLOAT8,
    "Speed85Avg" FLOAT8,
    "OccupancyPer" TEXT,
    "OccupancyPrc" FLOAT8,
    "OccupancySum" FLOAT8,
    "RangeStart" TIMESTAMPTZ,
    "RangeEnd" TIMESTAMPTZ,
    "RangeValue" INT,
    "CreatedAt" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT "FK_PuidData_Puid" FOREIGN KEY ("PuidId") REFERENCES "Puid"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_PuidData_PuidId" ON "PuidData" ("PuidId");
CREATE INDEX IF NOT EXISTS "IX_PuidData_RangeStart" ON "PuidData" ("RangeStart");


-- public."PuidResults" definition

-- Drop table

-- DROP TABLE public."PuidResults";

CREATE TABLE public."PuidResults" (
	"Id" serial4 NOT NULL,
	"PuidId" int4 NOT NULL,
	"CheckedAt" timestamptz NOT NULL,
	"StatusCode" int4 NOT NULL,
	"ResponseBody" jsonb NULL,
	"ResponseTimeMs" int8 NOT NULL,
	"IsSuccess" bool NOT NULL,
	CONSTRAINT "PuidResults_pkey" PRIMARY KEY ("Id")
);


-- public."PuidResults" foreign keys

ALTER TABLE public."PuidResults" ADD CONSTRAINT "PuidResults_PuidId_fkey" FOREIGN KEY ("PuidId") REFERENCES public."Puid"("Id") ON DELETE CASCADE;

INSERT INTO public."Puid"
( "SensorType", "MonitoringPostId", "Longitude", "Latitude", "SerialNumber", "EndPointsName", "Url", "LastActivityUTC", "CreatedAt", "IsActive")
VALUES
( 'PUID', 1, 0, 0, 'sn', 'puid_001', 'http://85.26.215.244:8180/api/integration/stat?email=userstats&password=userstats&sensor_id=b0ffd7f7-7ad9-4021-8c0a-e9d5b547639f&interval=3600', NULL, now(), true);

