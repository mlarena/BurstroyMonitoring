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
     'http://192.168.1.10/json',  
     true),
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'MUEKS'),    
     1, 
     'MUEKS_01',      
     'Датчик температеры грунта',    
     'http://192.168.1.14/json',  
     true),
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'IWS'), 
     1, 
     'IWS_01',   
     'Датчик комплекный параметров атмосферы', 
     'http://192.168.1.16/json',  
     true),
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'DUST'), 
     1, 
     'DUST_01',   
     'Датчик концентрации пыли', 
     'http://192.168.1.11/json',  
     false),    
    
    ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'DSPD'),    
     1, 
     'DSPD_01',
     'Датчик состояния дорожного полотна 1',
     'http://192.168.1.9/json',  
     true),
     
     ((SELECT "Id" FROM public."SensorType" WHERE "SensorTypeName" = 'DSPD'),    
     1, 
     'DSPD_02',
     'Датчик состояния дорожного полотна 2',
     'http://192.168.1.8/json',  
     true);