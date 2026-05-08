-- public.vw_meteo_standart_last source

CREATE OR REPLACE VIEW public.vw_meteo_standart_last
AS 
SELECT ps."Id" AS "PollingSessionId",
    ps."MonitoringPostId",
    ps."StartedAt",
    ps."CompletedAt",
    ps."Status",
    dov."VisibleRange",
    iws."EnvTemperature",
    iws."Humidity",
    iws."DewPoint",
    iws."PressureHPa",
    iws."PressureQNHHPa",
    iws."PressureMmHg",
    iws."WindSpeed",
    iws."WindDirection",
    iws."WindVSound",
    iws."PrecipitationType",
    iws."PrecipitationIntensity",
    iws."PrecipitationQuantity",
    iws."PrecipitationElapsed",
    iws."PrecipitationPeriod",
    dspd."Grip",
    dspd."Shake",
    dspd."UPower",
    dspd."TemperatureCase",
    dspd."TemperatureRoad",
    dspd."HeightH2O",
    dspd."HeightIce",
    dspd."HeightSnow",
    dspd."PercentICE",
    dspd."PercentPGM",
    dspd."RoadStatus",
    dspd."AngleToRoad",
    dspd."TemperatureFreezePGM",
    mueks."OwenCh1" 
   FROM "PollingSessions" ps
     LEFT JOIN "DOVData" dov ON ps."Id" = dov."PollingSessionId"
     LEFT JOIN "IWSData" iws ON ps."Id" = iws."PollingSessionId"
     LEFT JOIN "DSPDData" dspd ON ps."Id" = dspd."PollingSessionId"
     LEFT JOIN "MUEKSData" mueks ON ps."Id" = mueks."PollingSessionId"
     
  WHERE ps."Id" = (( SELECT "PollingSessions"."Id"
           FROM "PollingSessions"
          WHERE "PollingSessions"."Status"::text <> 'IN_PROGRESS'::text
          ORDER BY "PollingSessions"."CompletedAt" DESC
         LIMIT 1))
  ORDER BY dov."Id", iws."Id", dspd."Id";