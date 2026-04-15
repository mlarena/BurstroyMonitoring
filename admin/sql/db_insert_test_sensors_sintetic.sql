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
     'http://192.168.3.43/json',  
     true),
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'MUEKS'),    
     1, 
     'MUEKS_01',      
     'Модуль управления электроснабжением',    
     'http://192.168.3.42/json',  
     true),
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'IWS'), 
     1, 
     'IWS_01',   
     'IWS_01', 
     'http://192.168.3.45/json',  
     true),
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'DUST'), 
     1, 
     'DUST_01',   
     'Датчик концентрации пыли', 
     'http://192.168.3.19:8086/json',  
     true),    
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'DSPD'),    
     1, 
     'DSPD_01',
     'Датчик состояния дорожного полотна',
     'http://192.168.3.28/json',  
     true);