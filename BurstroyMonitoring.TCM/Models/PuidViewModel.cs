using System;
using System.Collections.Generic;
using System.Linq;
using BurstroyMonitoring.Data.Models;

namespace BurstroyMonitoring.TCM.Models;

public class PuidViewModel
{
    public List<PuidDataGroup> GroupedItems { get; set; } = new();
    public List<Puid> AvailablePuids { get; set; } = new();
    public string? CurrentEndPointName { get; set; }
    public string? CurrentSensorName { get; set; }
    public int? SelectedPuidId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    
    // Пагинация
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 10; // Интервалов на страницу
    
    public TrafficStatistics TotalStats { get; set; } = new();
    public TrafficStatistics AverageStats { get; set; } = new();
}

public class PuidDataGroup
{
    public DateTime RangeStart { get; set; }
    public DateTime RangeEnd { get; set; }
    public Guid MessageId { get; set; }
    public List<PuidData> Lanes { get; set; } = new();
    
    // Агрегаты для группы (интервала)
    public int TotalVolume => Lanes.Sum(l => l.Volume);
    public double AvgSpeed => Lanes.Any(l => l.Volume > 0) ? Lanes.Where(l => l.Volume > 0).Average(l => l.SpeedAvg) : 0;
}

public class TrafficStatistics
{
    public long TotalVolume { get; set; }
    public double AvgSpeed { get; set; }
    public double AvgOccupancy { get; set; }
    public long Class0 { get; set; }
    public long Class1 { get; set; }
    public long Class2 { get; set; }
    public long Class3 { get; set; }
    public long Class4 { get; set; }
    public long Class5 { get; set; }
}
