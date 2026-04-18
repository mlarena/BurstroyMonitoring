INSERT INTO public."Sensor" 
    ("SensorTypeId", 
     "MonitoringPostId", 
     "SerialNumber", 
     "EndPointsName", 
     "Url", 
     "IsActive")
VALUES
     ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'DOV'),    
     1, 
     'DOV_01',   
     'Датчик оптической видимости',    
     'http://localhost:5055/api/Sensors/DOV',  
     true),
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'MUEKS'),    
     1, 
     'MUEKS_01',      
     'Модуль управления электроснабжением',    
     'http://localhost:5055/api/Sensors/v2/MUEKS',  
     true),
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'IWS'), 
     1, 
     'IWS_01',   
     'IWS_01', 
     'http://localhost:5055/api/Sensors/IWS',  
     true),
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'DUST'), 
     1, 
     'DUST_01',   
     'Датчик концентрации пыли', 
     'http://localhost:5055/api/Sensors/DUST',  
     true),    
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'DSPD'),    
     1, 
     'DSPD_01',
     'Датчик состояния дорожного полотна',
     'http://localhost:5055/api/Sensors/DSPD',  
     true);