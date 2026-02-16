using System.Text.Json;
using System.Text.Json.Serialization;

namespace BurstroyMonitoring.Data.Models.JsonModels;

/// <summary>
/// Модель для десериализации JSON от датчиков IWS
/// </summary>
public class IwsJsonModel
{
    [JsonPropertyName("Serial")]
    public string? Serial { get; set; }
    
    [JsonPropertyName("Packet")]
    [JsonConverter(typeof(IwsPacketConverter))]
    public IwsPacket? Packet { get; set; }
}

public class IwsPacket
{
    public string? DataTime { get; set; }
    public decimal? EnvTemperature { get; set; }
    public decimal? Humidity { get; set; }
    public decimal? DewPoint { get; set; }
    public decimal? PressureHPa { get; set; }
    public decimal? PressureQNHHPa { get; set; }
    public decimal? PressureMmHg { get; set; }
    public decimal? WindSpeed { get; set; }
    public decimal? WindDirection { get; set; }
    public decimal? WindVSound { get; set; }
    public int? PrecipitationType { get; set; }
    public decimal? PrecipitationIntensity { get; set; }
    public decimal? PrecipitationQuantity { get; set; }
    public int? PrecipitationElapsed { get; set; }
    public int? PrecipitationPeriod { get; set; }
    public decimal? CO2Level { get; set; }
    public decimal? SupplyVoltage { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Altitude { get; set; }
    public int? KSP { get; set; }
    public decimal? GPSSpeed { get; set; }
    public decimal? AccelerationStDev { get; set; }
    public decimal? Roll { get; set; }
    public decimal? Pitch { get; set; }
    public int? WeAreFine { get; set; }
}

public class IwsPacketConverter : JsonConverter<IwsPacket>
{
    public override IwsPacket Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var packet = new IwsPacket();
        
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
                    case "datatime":
                        packet.DataTime = reader.GetString();
                        break;
                    case "envtemperature":
                        packet.EnvTemperature = GetNullableDecimal(ref reader);
                        break;
                    case "humidity":
                        packet.Humidity = GetNullableDecimal(ref reader);
                        break;
                    case "dewpoint":
                        packet.DewPoint = GetNullableDecimal(ref reader);
                        break;
                    case "pressure_hpa":
                        packet.PressureHPa = GetNullableDecimal(ref reader);
                        break;
                    case "pressure_qnh_hpa":
                        packet.PressureQNHHPa = GetNullableDecimal(ref reader);
                        break;
                    case "pressure_mm_hg":
                        packet.PressureMmHg = GetNullableDecimal(ref reader);
                        break;
                    case "windspeed":
                        packet.WindSpeed = GetNullableDecimal(ref reader);
                        break;
                    case "winddirection":
                        packet.WindDirection = GetNullableDecimal(ref reader);
                        break;
                    case "windvsound":
                        packet.WindVSound = GetNullableDecimal(ref reader);
                        break;
                    case "precipitationtype":
                        packet.PrecipitationType = GetNullableInt(ref reader);
                        break;
                    case "precipitationintensity":
                        packet.PrecipitationIntensity = GetNullableDecimal(ref reader);
                        break;
                    case "precipitationquantity":
                        packet.PrecipitationQuantity = GetNullableDecimal(ref reader);
                        break;
                    case "precipitationelaps":
                        packet.PrecipitationElapsed = GetNullableInt(ref reader);
                        break;
                    case "precipitationperiod":
                        packet.PrecipitationPeriod = GetNullableInt(ref reader);
                        break;
                    case "co2level":  
                        packet.CO2Level = GetNullableDecimal(ref reader);
                        break;    
                    case "supplyvoltage":
                        packet.SupplyVoltage = GetNullableDecimal(ref reader);
                        break;
                    case "latitude":
                        packet.Latitude = GetNullableDecimal(ref reader);
                        break;
                    case "longitude":
                        packet.Longitude = GetNullableDecimal(ref reader);
                        break;
                    case "altitude":
                        packet.Altitude = GetNullableDecimal(ref reader);
                        break;
                    case "ksp":
                        packet.KSP = GetNullableInt(ref reader);
                        break;
                    case "gps_speed":
                        packet.GPSSpeed = GetNullableDecimal(ref reader);
                        break;
                    case "accelerationstdev":
                        packet.AccelerationStDev = GetNullableDecimal(ref reader);
                        break;
                    case "roll":
                        packet.Roll = GetNullableDecimal(ref reader);
                        break;
                    case "pitch":
                        packet.Pitch = GetNullableDecimal(ref reader);
                        break;
                    case "wearefine":
                        packet.WeAreFine = GetNullableInt(ref reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }
        
        throw new JsonException("Unexpected end when reading IwsPacket");
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
    
    public override void Write(Utf8JsonWriter writer, IwsPacket value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}