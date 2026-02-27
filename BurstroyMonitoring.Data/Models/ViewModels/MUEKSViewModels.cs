// BurstroyMonitoring.Data/Models/ViewModels/MUEKSViewModels.cs
using System;
using System.Collections.Generic;

namespace BurstroyMonitoring.Data.Models.ViewModels
{
    public class MUEKSMeasurementViewModel
    {
        public int MueksDataId { get; set; }
        public DateTime ReceivedAt { get; set; }        
        public DateTime DataTimestamp { get; set; }
        public decimal? TemperatureBox { get; set; }
        public decimal? VoltagePowerIn12b { get; set; }
        public decimal? VoltageOut12b { get; set; }
        public decimal? VoltageAkb { get; set; }
        public decimal? CurrentOut12b { get; set; }
        public decimal? CurrentOut48b { get; set; }
        public decimal? CurrentAkb { get; set; }
        public decimal? WattHoursAkb { get; set; }
        public decimal? VisibleRange { get; set; }
        public int? Sensor220b { get; set; }
        public int? DoorStatus { get; set; }
        public string? TdsH { get; set; }
        public string? TdsTds { get; set; }
        public string? TkosaT1 { get; set; }
        public string? TkosaT3 { get; set; }
    }

    public class MUEKSDataViewModel
    {
        public int SensorId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string EndpointName { get; set; } = string.Empty;
        public string PostName { get; set; } = string.Empty;
        public List<MUEKSMeasurementViewModel> Measurements { get; set; } = new();
    }

    public class MUEKSParameterInfo
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
        public bool IsText { get; set; }
    }
}