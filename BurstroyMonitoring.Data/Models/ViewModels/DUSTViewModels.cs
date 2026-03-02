// BurstroyMonitoring.Data/Models/ViewModels/DUSTViewModels.cs
using System;
using System.Collections.Generic;

namespace BurstroyMonitoring.Data.Models.ViewModels
{
    public class DUSTMeasurementViewModel
    {
        public int DustDataId { get; set; }
        public DateTime ReceivedAt { get; set; }        
        public decimal? Pm10Act { get; set; }
        public decimal? Pm25Act { get; set; }
        public decimal? Pm1Act { get; set; }
        public decimal? Pm10Awg { get; set; }
        public decimal? Pm25Awg { get; set; }
        public decimal? Pm1Awg { get; set; }
        public decimal? FlowProbe { get; set; }
        public decimal? TemperatureProbe { get; set; }
        public decimal? HumidityProbe { get; set; }
        public int? LaserStatus { get; set; }
        public decimal? SupplyVoltage { get; set; }
    }

    public class DUSTDataViewModel
    {
        public int SensorId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string EndpointName { get; set; } = string.Empty;
        public string PostName { get; set; } = string.Empty;
        public List<DUSTMeasurementViewModel> Measurements { get; set; } = new();
    }

    public class DUSTParameterInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public int Order { get; set; }
        public string Group { get; set; } = string.Empty;
    }
}