using System.Text.Json;
using System.Text.Json.Serialization;

namespace BurstroyMonitoring.Data.Models.JsonModels;

/// <summary>
/// Модель для десериализации JSON от датчиков DSPD
/// </summary>
public class DspdJsonModel
{
    [JsonPropertyName("Serial")]
    public string? Serial { get; set; }
    
    [JsonPropertyName("Packet")]
    [JsonConverter(typeof(DspdPacketConverter))]
    public DspdPacket? Packet { get; set; }
}

public class DspdPacket
{
    public decimal? Grip { get; set; }
    public decimal? Shake { get; set; }
    public decimal? UPower { get; set; }
    public string? DataTime { get; set; }
    public decimal? TemperatureCase { get; set; }
    public decimal? TemperatureRoad { get; set; }
    public decimal? HeightH2O { get; set; }
    public decimal? HeightIce { get; set; }
    public decimal? HeightSnow { get; set; }
    public decimal? PercentICE { get; set; }
    public decimal? PercentPGM { get; set; }
    public int? RoadStatus { get; set; }
    public decimal? AngleToRoad { get; set; }
    public decimal? TemperatureFreezePGM { get; set; }
    public int? NeedCalibration { get; set; }
    public object? GPSLatitude { get; set; }
    public object? GPSLongitude { get; set; }
    public decimal? DistanceToSurface { get; set; }
}

public class DspdPacketConverter : JsonConverter<DspdPacket>
{
    public override DspdPacket Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var packet = new DspdPacket();
        
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return packet;
            
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString()?.ToLowerInvariant();
                reader.Read();
                
                switch (propertyName)
                {
                    case "grip":
                        packet.Grip = GetNullableDecimal(ref reader);
                        break;
                    case "shake":
                        packet.Shake = GetNullableDecimal(ref reader);
                        break;
                    case "u_power":
                        packet.UPower = GetNullableDecimal(ref reader);
                        break;
                    case "datatime":
                        packet.DataTime = reader.GetString();
                        break;
                    case "temp_case":
                    case "temperature_case":
                        packet.TemperatureCase = GetNullableDecimal(ref reader);
                        break;
                    case "temp_road":
                    case "temperature_road":
                        packet.TemperatureRoad = GetNullableDecimal(ref reader);
                        break;
                    case "height_h2o":
                        packet.HeightH2O = GetNullableDecimal(ref reader);
                        break;
                    case "height_ice":
                        packet.HeightIce = GetNullableDecimal(ref reader);
                        break;
                    case "height_snow":
                        packet.HeightSnow = GetNullableDecimal(ref reader);
                        break;
                    case "percent_ice":
                        packet.PercentICE = GetNullableDecimal(ref reader);
                        break;
                    case "percent_pgm":
                        packet.PercentPGM = GetNullableDecimal(ref reader);
                        break;
                    case "road_status":
                        packet.RoadStatus = GetNullableInt(ref reader);
                        break;
                    case "angle_to_road":
                        packet.AngleToRoad = GetNullableDecimal(ref reader);
                        break;
                    case "temp_frize_pgm":
                    case "temperature_frize_pgm":
                        packet.TemperatureFreezePGM = GetNullableDecimal(ref reader);
                        break;
                    case "need_calibration":
                        packet.NeedCalibration = GetNullableInt(ref reader);
                        break;
                    case "gps_latitude":
                        packet.GPSLatitude = GetObjectValue(ref reader);
                        break;
                    case "gps_longitude":
                        packet.GPSLongitude = GetObjectValue(ref reader);
                        break;
                    case "distance_to_surface":
                        packet.DistanceToSurface = GetNullableDecimal(ref reader);
                        break;    
                        
                    default:
                        reader.Skip();
                        break;
                }
            }
        }
        
        throw new JsonException("Unexpected end when reading DspdPacket");
    }
    
    private decimal? GetNullableDecimal(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (decimal.TryParse(str, out var result))
                return result;
            return null;
        }
        
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDecimal();
        
        return null;
    }
    
    private int? GetNullableInt(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (int.TryParse(str, out var result))
                return result;
            return null;
        }
        
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt32();
        
        return null;
    }
    
    private object? GetObjectValue(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                return reader.GetDecimal();
            default:
                reader.Skip();
                return null;
        }
    }
    
    public override void Write(Utf8JsonWriter writer, DspdPacket value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}