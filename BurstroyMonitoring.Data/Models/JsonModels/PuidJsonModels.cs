namespace BurstroyMonitoring.Data.Models.JsonModels;

public class SmartRoadPuidResult
{
    public string message_id { get; set; } = string.Empty;
    public List<MessageData> message_data { get; set; } = new();
}

public class MessageData
{
    public string sensor_id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public bool connected { get; set; }
    public List<int> lane_direction { get; set; } = new();
    public int direction { get; set; }
    public List<Datum> data { get; set; } = new();
}

public class Datum
{
    public int range_value { get; set; }
    public DateTime range_start { get; set; }
    public DateTime range_end { get; set; }
    public List<Lane> lanes { get; set; } = new();
}

public class Lane
{
    public int lane { get; set; }
    public int volume { get; set; }
    public int class_0 { get; set; }
    public int class_1 { get; set; }
    public int class_2 { get; set; }
    public int class_3 { get; set; }
    public int class_4 { get; set; }
    public int class_5 { get; set; }
    public double gap_avg { get; set; }
    public double gap_sum { get; set; }
    public double speed_avg { get; set; }
    public double headway_avg { get; set; }
    public double headway_sum { get; set; }
    public double speed85_avg { get; set; }
    public string occupancy_per { get; set; } = string.Empty;
    public double occupancy_prc { get; set; }
    public double occupancy_sum { get; set; }
}
