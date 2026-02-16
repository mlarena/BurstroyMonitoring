namespace BurstroyMonitoring.Data.Models.ViewModels
{
    public class MapViewModel
    {
        public List<MonitoringPostMarker> Posts { get; set; } = new();
        public List<SensorMarker> Sensors { get; set; } = new();
        public FilterOptions Filters { get; set; } = new();
    }

    public class MonitoringPostMarker
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsMobile { get; set; }
        public bool IsActive { get; set; }
        public int SensorCount { get; set; }
        public string IconColor => IsMobile ? "blue" : "green";
        public string IconType => IsMobile ? "truck" : "building";
    }

    public class SensorMarker
    {
        public int Id { get; set; }
        public int SensorTypeId { get; set; }
        public string SensorTypeName { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string EndPointsName { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsActive { get; set; }
        public int? MonitoringPostId { get; set; }
        public string? MonitoringPostName { get; set; }
        public string IconColor => IsActive ? "orange" : "red";
        public string SensorTypeIcon => GetSensorTypeIcon();
        
        private string GetSensorTypeIcon()
        {
            return SensorTypeName.ToUpper() switch
            {
                "DSPD" => "snowflake",
                "IWS" => "wind",
                "DUST" => "cloud",
                "DOV" => "eye",
                "MUEKS" => "microchip",
                _ => "dot-circle"
            };
        }
    }

    public class FilterOptions
    {
        public bool ShowActivePosts { get; set; } = true;
        public bool ShowInactivePosts { get; set; } = false;
        public bool ShowActiveSensors { get; set; } = true;
        public bool ShowInactiveSensors { get; set; } = false;
        public bool ShowMobilePosts { get; set; } = true;
        public bool ShowStationaryPosts { get; set; } = true;
        public int? SelectedPostId { get; set; }
        public string? SelectedSensorType { get; set; }
    }
}