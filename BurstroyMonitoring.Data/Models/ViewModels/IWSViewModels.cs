// BurstroyMonitoring.Data/Models/ViewModels/IWSViewModels.cs
using System;
using System.Collections.Generic;

namespace BurstroyMonitoring.Data.Models.ViewModels
{
    public class IWSMeasurementViewModel
    {
        public int IwsDataId { get; set; }
        public DateTime ReceivedAt { get; set; }        
        public decimal? EnvironmentTemperature { get; set; }
        public decimal? HumidityPercentage { get; set; }
        public decimal? DewPoint { get; set; }
        public decimal? PressureHpa { get; set; }
        public decimal? PressureQNHHpa { get; set; }
        public decimal? PressureMmHg { get; set; }
        public decimal? WindSpeed { get; set; }
        public decimal? WindDirection { get; set; }
        public decimal? WindVSound { get; set; }
        public int? PrecipitationType { get; set; }
        public decimal? PrecipitationIntensity { get; set; }
        public decimal? PrecipitationQuantity { get; set; }
        public int? PrecipitationElapsed { get; set; }
        public int? PrecipitationPeriod { get; set; }
        public decimal? Co2Level { get; set; }
        public decimal? SupplyVoltage { get; set; }
        public decimal? IwsLatitude { get; set; }
        public decimal? IwsLongitude { get; set; }
        public decimal? Altitude { get; set; }
        public decimal? GpsSpeed { get; set; }
        public int? KspValue { get; set; }
        public decimal? AccelerationStdDev { get; set; }
        public decimal? RollAngle { get; set; }
        public decimal? PitchAngle { get; set; }
    }

    public class IWSDataViewModel
    {
        public int SensorId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string EndpointName { get; set; } = string.Empty;
        public string PostName { get; set; } = string.Empty;
        public List<IWSMeasurementViewModel> Measurements { get; set; } = new();
    }

    public class IWSParameterInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public int Order { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}