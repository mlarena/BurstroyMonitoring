-- public.vw_dov_data_full source
CREATE OR REPLACE VIEW public.vw_dov_data_full
AS SELECT 
    -- DOVData (основная таблица, ВСЕ поля)
    dd."Id" AS dov_data_id,
    dd."ReceivedAt" AS received_at,
    dd."DataTimestamp" AS data_timestamp,
    dd."VisibleRange" AS visible_range,
    dd."BrightFlag" AS bright_flag,
    -- Sensor
    s."Id" AS sensor_id,
    s."Longitude" AS sensor_longitude,
    s."Latitude" AS sensor_latitude,
    s."SerialNumber" AS serial_number,
    s."EndPointsName" AS endpoint_name,
    s."Url" AS sensor_url,
    s."CheckIntervalSeconds" AS check_interval_seconds,
    s."LastActivityUTC" AS last_activity_utc,
    s."IsActive" AS sensor_is_active,
    -- SensorType
    st."Id" AS sensor_type_id,
    st."SensorTypeName" AS sensor_type_name,
    st."Description" AS sensor_type_description,
    -- MonitoringPost
    mp."Id" AS post_id,
    mp."Name" AS post_name,
    mp."Address" AS post_address,
    mp."Description" AS post_description,
    mp."IsMobile" AS post_is_mobile,
    mp."IsActive" AS post_is_active
FROM "DOVData" dd
LEFT JOIN "Sensor" s ON dd."SensorId" = s."Id"
LEFT JOIN "SensorType" st ON s."SensorTypeId" = st."Id"
LEFT JOIN "MonitoringPost" mp ON s."MonitoringPostId" = mp."Id";

-- public.vw_dspd_data_full source
CREATE OR REPLACE VIEW public.vw_dspd_data_full
AS SELECT 
    -- DSPDData (основная таблица, ВСЕ поля)
    dd."Id" AS dspd_data_id,
    dd."ReceivedAt" AS received_at,
    dd."DataTimestamp" AS data_timestamp,
    dd."Grip" AS grip_coefficient,
    dd."Shake" AS shake_level,
    dd."UPower" AS voltage_power,
    dd."TemperatureCase" AS case_temperature,
    dd."TemperatureRoad" AS road_temperature,
    dd."HeightH2O" AS water_height,
    dd."HeightIce" AS ice_height,
    dd."HeightSnow" AS snow_height,
    dd."PercentICE" AS ice_percentage,
    dd."PercentPGM" AS pgm_percentage,
    dd."RoadStatus" AS road_status_code,
    dd."AngleToRoad" AS road_angle,
    dd."TemperatureFreezePGM" AS freeze_temperature,
    dd."NeedCalibration" AS calibration_needed,
    dd."GPSLatitude" AS gps_latitude,
    dd."GPSLongitude" AS gps_longitude,
    dd."IsGpsValid" AS gps_valid,
    dd."DistanceToSurface" AS distance_to_surface,
    -- Sensor
    s."Id" AS sensor_id,
    s."Longitude" AS sensor_longitude,
    s."Latitude" AS sensor_latitude,
    s."SerialNumber" AS serial_number,
    s."EndPointsName" AS endpoint_name,
    s."Url" AS sensor_url,
    s."CheckIntervalSeconds" AS check_interval_seconds,
    s."LastActivityUTC" AS last_activity_utc,
    s."IsActive" AS sensor_is_active,
    -- SensorType
    st."Id" AS sensor_type_id,
    st."SensorTypeName" AS sensor_type_name,
    st."Description" AS sensor_type_description,
    -- MonitoringPost
    mp."Id" AS post_id,
    mp."Name" AS post_name,
    mp."Address" AS post_address,
    mp."Description" AS post_description,
    mp."IsMobile" AS post_is_mobile,
    mp."IsActive" AS post_is_active
FROM "DSPDData" dd
LEFT JOIN "Sensor" s ON dd."SensorId" = s."Id"
LEFT JOIN "SensorType" st ON s."SensorTypeId" = st."Id"
LEFT JOIN "MonitoringPost" mp ON s."MonitoringPostId" = mp."Id";

-- public.vw_dust_data_full source
CREATE OR REPLACE VIEW public.vw_dust_data_full
AS SELECT 
    -- DustData (основная таблица, ВСЕ поля)
    dd."Id" AS dust_data_id,
    dd."ReceivedAt" AS received_at,
    dd."DataTimestamp" AS data_timestamp,
    dd."PM10Act" AS pm10act,
    dd."PM25Act" AS pm25act,
    dd."PM1Act" AS pm1act,
    dd."PM10AWG" AS pm10awg,
    dd."PM25AWG" AS pm25awg,
    dd."PM1AWG" AS pm1awg,
    dd."FlowProbe" AS flowprobe,
    dd."TemperatureProbe" AS temperatureprobe,
    dd."HumidityProbe" AS humidityprobe,
    dd."LaserStatus" AS laserstatus,
    dd."SupplyVoltage" AS supplyvoltage,
    -- Sensor
    s."Id" AS sensor_id,
    s."Longitude" AS sensor_longitude,
    s."Latitude" AS sensor_latitude,
    s."SerialNumber" AS serial_number,
    s."EndPointsName" AS endpoint_name,
    s."Url" AS sensor_url,
    s."CheckIntervalSeconds" AS check_interval_seconds,
    s."LastActivityUTC" AS last_activity_utc,
    s."IsActive" AS sensor_is_active,
    -- SensorType
    st."Id" AS sensor_type_id,
    st."SensorTypeName" AS sensor_type_name,
    st."Description" AS sensor_type_description,
    -- MonitoringPost
    mp."Id" AS post_id,
    mp."Name" AS post_name,
    mp."Address" AS post_address,
    mp."Description" AS post_description,
    mp."IsMobile" AS post_is_mobile,
    mp."IsActive" AS post_is_active
FROM "DustData" dd
LEFT JOIN "Sensor" s ON dd."SensorId" = s."Id"
LEFT JOIN "SensorType" st ON s."SensorTypeId" = st."Id"
LEFT JOIN "MonitoringPost" mp ON s."MonitoringPostId" = mp."Id";

-- public.vw_iws_data_full source
CREATE OR REPLACE VIEW public.vw_iws_data_full
AS SELECT 
    -- IWSData (основная таблица, ВСЕ поля)
    iws."Id" AS iws_data_id,
    iws."ReceivedAt" AS received_at,
    iws."DataTimestamp" AS data_timestamp,
    iws."EnvTemperature" AS environment_temperature,
    iws."Humidity" AS humidity_percentage,
    iws."DewPoint" AS dew_point,
    iws."PressureHPa" AS pressure_hpa,
    iws."PressureQNHHPa" AS pressure_qnh_hpa,
    iws."PressureMmHg" AS pressure_mmhg,
    iws."WindSpeed" AS wind_speed,
    iws."WindDirection" AS wind_direction,
    iws."WindVSound" AS wind_vs_sound,
    iws."PrecipitationType" AS precipitation_type,
    iws."PrecipitationIntensity" AS precipitation_intensity,
    iws."PrecipitationQuantity" AS precipitation_quantity,
    iws."PrecipitationElapsed" AS precipitation_elapsed,
    iws."PrecipitationPeriod" AS precipitation_period,
    iws."CO2Level" AS co2_level,
    iws."SupplyVoltage" AS supply_voltage,
    iws."Latitude" AS iws_latitude,
    iws."Longitude" AS iws_longitude,
    iws."Altitude" AS altitude,
    iws."KSP" AS ksp_value,
    iws."GPSSpeed" AS gps_speed,
    iws."AccelerationStDev" AS acceleration_std_dev,
    iws."Roll" AS roll_angle,
    iws."Pitch" AS pitch_angle,
    iws."WeAreFine" AS status_ok,
    -- Sensor
    s."Id" AS sensor_id,
    s."Longitude" AS sensor_longitude,
    s."Latitude" AS sensor_latitude,
    s."SerialNumber" AS serial_number,
    s."EndPointsName" AS endpoint_name,
    s."Url" AS sensor_url,
    s."CheckIntervalSeconds" AS check_interval_seconds,
    s."LastActivityUTC" AS last_activity_utc,
    s."IsActive" AS sensor_is_active,
    -- SensorType
    st."Id" AS sensor_type_id,
    st."SensorTypeName" AS sensor_type_name,
    st."Description" AS sensor_type_description,
    -- MonitoringPost
    mp."Id" AS post_id,
    mp."Name" AS post_name,
    mp."Address" AS post_address,
    mp."Description" AS post_description,
    mp."IsMobile" AS post_is_mobile,
    mp."IsActive" AS post_is_active
FROM "IWSData" iws
LEFT JOIN "Sensor" s ON iws."SensorId" = s."Id"
LEFT JOIN "SensorType" st ON s."SensorTypeId" = st."Id"
LEFT JOIN "MonitoringPost" mp ON s."MonitoringPostId" = mp."Id";

-- public.vw_mueks_data_full source
CREATE OR REPLACE VIEW public.vw_mueks_data_full
AS SELECT 
    -- MUEKSData (основная таблица, ВСЕ поля)
    pd."Id" AS mueks_data_id,
    pd."ReceivedAt" AS received_at,
    pd."DataTimestamp" AS data_timestamp,
    pd."TemperatureBox" AS temperature_box,
    pd."UPowerIn12B" AS voltage_power_in_12b,
    pd."UOut12B" AS voltage_out_12b,
    pd."IOut12B" AS current_out_12b,
    pd."IOut48B" AS current_out_48b,
    pd."UAkb" AS voltage_akb,
    pd."IAkb" AS current_akb,
    pd."Sens220B" AS sensor_220b,
    pd."WhAkb" AS watt_hours_akb,
    pd."VisibleRange" AS visible_range,
    pd."DoorStatus" AS door_status,
    pd."TdsH" AS tds_h,
    pd."TdsTds" AS tds_tds,
    pd."TkosaT1" AS tkosa_t1,
    pd."TkosaT3" AS tkosa_t3,
    -- Sensor
    s."Id" AS sensor_id,
    s."Longitude" AS sensor_longitude,
    s."Latitude" AS sensor_latitude,
    s."SerialNumber" AS serial_number,
    s."EndPointsName" AS endpoint_name,
    s."Url" AS sensor_url,
    s."CheckIntervalSeconds" AS check_interval_seconds,
    s."LastActivityUTC" AS last_activity_utc,
    s."IsActive" AS sensor_is_active,
    -- SensorType
    st."Id" AS sensor_type_id,
    st."SensorTypeName" AS sensor_type_name,
    st."Description" AS sensor_type_description,
    -- MonitoringPost
    mp."Id" AS post_id,
    mp."Name" AS post_name,
    mp."Address" AS post_address,
    mp."Description" AS post_description,
    mp."IsMobile" AS post_is_mobile,
    mp."IsActive" AS post_is_active
FROM "MUEKSData" pd
LEFT JOIN "Sensor" s ON pd."SensorId" = s."Id"
LEFT JOIN "SensorType" st ON s."SensorTypeId" = st."Id"
LEFT JOIN "MonitoringPost" mp ON s."MonitoringPostId" = mp."Id";

-- public.vw_sensor_errors_full source
CREATE OR REPLACE VIEW public.vw_sensor_errors_full
AS SELECT 
    -- SensorError (основная таблица, ВСЕ поля)
    se."Id" AS error_id,
    se."ErrorLevel" AS error_level,
    se."ErrorMessage" AS error_message,
    se."StackTrace" AS stack_trace,
    se."ErrorSource" AS error_source,
    se."ExceptionType" AS exception_type,
    se."CreatedAt" AS error_created_at,
    se."AdditionalData" AS additional_data,
    -- Sensor
    s."Id" AS sensor_id,
    s."Longitude" AS sensor_longitude,
    s."Latitude" AS sensor_latitude,
    s."SerialNumber" AS serial_number,
    s."EndPointsName" AS endpoint_name,
    s."Url" AS sensor_url,
    s."CheckIntervalSeconds" AS check_interval_seconds,
    s."LastActivityUTC" AS last_activity_utc,
    s."IsActive" AS sensor_is_active,
    -- SensorType
    st."Id" AS sensor_type_id,
    st."SensorTypeName" AS sensor_type_name,
    st."Description" AS sensor_type_description,
    -- MonitoringPost
    mp."Id" AS post_id,
    mp."Name" AS post_name,
    mp."Address" AS post_address,
    mp."Description" AS post_description,
    mp."Longitude" AS post_longitude,
    mp."Latitude" AS post_latitude,
    mp."IsMobile" AS post_is_mobile,
    mp."IsActive" AS post_is_active,
    mp."UpdatedAt" AS post_updated_at
FROM "SensorError" se
LEFT JOIN "Sensor" s ON se."SensorId" = s."Id"
LEFT JOIN "SensorType" st ON s."SensorTypeId" = st."Id"
LEFT JOIN "MonitoringPost" mp ON s."MonitoringPostId" = mp."Id";

-- public.vw_sensor_results_full source
CREATE OR REPLACE VIEW public.vw_sensor_results_full
AS SELECT 
    -- SensorResults (основная таблица, ВСЕ поля)
    sr."Id" AS result_id,
    sr."CheckedAt" AS checked_at,
    sr."StatusCode" AS status_code,
    sr."ResponseBody" AS response_body,
    sr."ResponseTimeMs" AS response_time_ms,
    sr."IsSuccess" AS is_success,
    -- Sensor
    s."Id" AS sensor_id,
    s."Longitude" AS sensor_longitude,
    s."Latitude" AS sensor_latitude,
    s."SerialNumber" AS serial_number,
    s."EndPointsName" AS endpoint_name,
    s."Url" AS sensor_url,
    s."CheckIntervalSeconds" AS check_interval_seconds,
    s."LastActivityUTC" AS last_activity_utc,
    s."IsActive" AS sensor_is_active,
    -- SensorType
    st."Id" AS sensor_type_id,
    st."SensorTypeName" AS sensor_type_name,
    st."Description" AS sensor_type_description,
    -- MonitoringPost
    mp."Id" AS post_id,
    mp."Name" AS post_name,
    mp."Address" AS post_address,
    mp."Description" AS post_description,
    mp."IsMobile" AS post_is_mobile,
    mp."IsActive" AS post_is_active
FROM "SensorResults" sr
LEFT JOIN "Sensor" s ON sr."SensorId" = s."Id"
LEFT JOIN "SensorType" st ON s."SensorTypeId" = st."Id"
LEFT JOIN "MonitoringPost" mp ON s."MonitoringPostId" = mp."Id";