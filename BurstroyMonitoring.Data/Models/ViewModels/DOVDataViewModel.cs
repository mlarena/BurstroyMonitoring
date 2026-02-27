// BurstroyMonitoring.Data/Models/ViewModels/DOVDataViewModel.cs
using System;
using System.Collections.Generic;

namespace BurstroyMonitoring.Data.Models.ViewModels
{
    public class DOVDataViewModel
    {
        public int SensorId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string EndpointName { get; set; } = string.Empty;
        public string? PostName { get; set; }
        public List<DOVMeasurementViewModel> Measurements { get; set; } = new();
    }

    public class DOVMeasurementViewModel
    {
        public int DovDataId { get; set; }
        public DateTime ReceivedAt { get; set; }        
        public DateTime DataTimestamp { get; set; }
        public decimal VisibleRange { get; set; }
        public int BrightFlag { get; set; }
        public int? SensorId { get; set; }
        public string? SerialNumber { get; set; }
        public string? EndpointName { get; set; }
    }
}