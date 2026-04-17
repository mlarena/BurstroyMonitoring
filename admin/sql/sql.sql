SELECT "Id", "Name", "Description", "Longitude", "Latitude", "IsMobile", "IsActive", "CreatedAt", "UpdatedAt", "Address", "PollingIntervalSeconds", "LastPolledAt"
FROM public."MonitoringPost";


--truncate table public."SensorResults";

--truncate table public."PollingSessions" CASCADE;
--truncate table public."SensorError"

SELECT sr."Id", sr."SensorId", sr."CheckedAt", sr."StatusCode", sr."ResponseBody", sr."ResponseTimeMs", sr."IsSuccess", s."EndPointsName" 
FROM public."SensorResults"  sr
inner join "Sensor" s ON sr."SensorId" = s."Id" 
order by "Id" desc limit 10; 


SELECT "Id", "SensorId", "ReceivedAt", "DataTimestamp", "VisibleRange", "BrightFlag", "PollingSessionId", "MonitoringPostId"
FROM public."DOVData" order by "Id" desc limit 10;


SELECT "Id", "SensorId", "ReceivedAt", "DataTimestamp", "Grip", "Shake", "UPower", "TemperatureCase", "TemperatureRoad", "HeightH2O", 
"HeightIce", "HeightSnow", "PercentICE", "PercentPGM", "RoadStatus", "AngleToRoad", "TemperatureFreezePGM", "NeedCalibration", "GPSLatitude",
"GPSLongitude", "DistanceToSurface", "IsGpsValid", "PollingSessionId", "MonitoringPostId"
FROM public."DSPDData" order by "Id" desc limit 10; 


SELECT "Id", "SensorId", "ReceivedAt", "DataTimestamp", "PM10Act", "PM25Act", "PM1Act", "PM10AWG", "PM25AWG", "PM1AWG", 
"FlowProbe", "TemperatureProbe", "HumidityProbe", "LaserStatus", "SupplyVoltage", "PollingSessionId", "MonitoringPostId"
FROM public."DustData" order by "Id" desc limit 10; 

SELECT "Id", "SensorId", "ReceivedAt", "DataTimestamp", "EnvTemperature", "Humidity", 
"DewPoint", "PressureHPa", "PressureQNHHPa", "PressureMmHg", "WindSpeed", "WindDirection", 
"WindVSound", "PrecipitationType", "PrecipitationIntensity", "PrecipitationQuantity", 
"PrecipitationElapsed", "PrecipitationPeriod", "CO2Level", "SupplyVoltage", "Latitude", 
"Longitude", "Altitude", "KSP", "GPSSpeed", "AccelerationStDev", "Roll", "Pitch", "WeAreFine", "PollingSessionId", "MonitoringPostId"
FROM public."IWSData" order by "Id" desc limit 10; 

SELECT "Id", "SensorId", "ReceivedAt", "DataTimestamp", "TemperatureBox", 
"UPowerIn12B", "UOut12B", "IOut12B", "IOut48B", "UAkb", "IAkb", "Sens220B", "WhAkb", "VisibleRange", "DoorStatus", "TdsH", "TdsTds", "TkosaT1", "TkosaT3", "PollingSessionId", "MonitoringPostId"
FROM public."MUEKSData" order by "Id" desc limit 10; 


SELECT "Id", "MonitoringPostId", "StartedAt", "CompletedAt", "Status", "TotalSensorsCount", "SuccessfulSensorsCount", "FailedSensorsDetails"
FROM public."PollingSessions"; 


SELECT "Id", "SensorId", "ErrorLevel", "ErrorMessage", "StackTrace", "ErrorSource", "ExceptionType", "CreatedAt", "AdditionalData"
FROM public."SensorError" order by "Id" desc limit 10; 

--посмотреть текущие подключения
SELECT pid, usename, application_name, client_addr, state 
FROM pg_stat_activity 
WHERE datname = 'sensordb';

--завершить подключения к текущей базе данных
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = 'sensordb_prod'
AND pid <> pg_backend_pid();


SELECT pid, usename, application_name, client_addr, state FROM pg_stat_activity WHERE datname = 'sensordb_prod';

INSERT INTO public."MonitoringPost"
( "Name", "Description", "Longitude", "Latitude", "IsMobile", "IsActive", "CreatedAt", "UpdatedAt", "Address", "PollingIntervalSeconds", "LastPolledAt")
VALUES(nextval( '', '', 0, 0, false, true, now(), now(), '', 60, '');
