// BurstroyMonitoring.Data/Models/ViewModels/DSPDViewModels.cs
using System;
using System.Collections.Generic;

namespace BurstroyMonitoring.Data.Models.ViewModels
{
    public class DSPDMeasurementViewModel
    {
        public int DspdDataId { get; set; }
        public DateTime DataTimestamp { get; set; }
        public decimal? GripCoefficient { get; set; }
        public decimal? ShakeLevel { get; set; }
        public decimal? VoltagePower { get; set; }
        public decimal? CaseTemperature { get; set; }
        public decimal? RoadTemperature { get; set; }
        public decimal? WaterHeight { get; set; }
        public decimal? IceHeight { get; set; }
        public decimal? SnowHeight { get; set; }
        public decimal? IcePercentage { get; set; }
        public decimal? PgmPercentage { get; set; }
        public int? RoadStatusCode { get; set; }
        public decimal? RoadAngle { get; set; }
        public decimal? FreezeTemperature { get; set; }
        public decimal? DistanceToSurface { get; set; }
        public int? CalibrationNeeded { get; set; }
        public decimal? GpsLatitude { get; set; }
        public decimal? GpsLongitude { get; set; }
        public bool? GpsValid { get; set; }
    }

    public class DSPDDataViewModel
    {
        public int SensorId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string EndpointName { get; set; } = string.Empty;
        public string PostName { get; set; } = string.Empty;
        public List<DSPDMeasurementViewModel> Measurements { get; set; } = new();
    }

    public class DSPDParameterInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public int Order { get; set; }
    }
}