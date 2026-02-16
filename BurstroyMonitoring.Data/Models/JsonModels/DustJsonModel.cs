using System.Text.Json;
using System.Text.Json.Serialization;

namespace BurstroyMonitoring.Data.Models.JsonModels;

/// <summary>
/// Модель для десериализации JSON от датчиков DUST
/// </summary>
public class DustJsonModel
{
    [JsonPropertyName("Serial")]
    public string? Serial { get; set; }
    
    [JsonPropertyName("Packet")]
    [JsonConverter(typeof(DustPacketConverter))]
    public DustPacket? Packet { get; set; }
}

public class DustPacket
{
    // Текущие значения PM
    public decimal? PM10_act { get; set; }
    public decimal? PM2_5_act { get; set; }
    public decimal? PM1_0_act { get; set; }
    
    // Средние значения PM
    public decimal? PM10_awg { get; set; }
    public decimal? PM2_5_awg { get; set; }
    public decimal? PM1_0_awg { get; set; }
    
    // Данные датчиков
    public decimal? Flow_probe { get; set; }
    public decimal? Temperature_probe { get; set; }
    public decimal? Humidity_probe { get; set; }
    
    // Статус и питание
    public int? Laser_status { get; set; }
    public decimal? Supply_voltage { get; set; }
}

public class DustPacketConverter : JsonConverter<DustPacket>
{
    public override DustPacket Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var packet = new DustPacket();
        
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
                    case "pm10_act":
                        packet.PM10_act = GetNullableDecimal(ref reader);
                        break;
                    case "pm2.5_act":
                    case "pm25_act":
                        packet.PM2_5_act = GetNullableDecimal(ref reader);
                        break;
                    case "pm1.0_act":
                        packet.PM1_0_act = GetNullableDecimal(ref reader);
                        break;
                    case "pm10_awg":
                        packet.PM10_awg = GetNullableDecimal(ref reader);
                        break;
                    case "pm2.5_awg":
                    case "pm25_awg":
                        packet.PM2_5_awg = GetNullableDecimal(ref reader);
                        break;
                    case "pm1.0_awg":
                        packet.PM1_0_awg = GetNullableDecimal(ref reader);
                        break;
                    case "flow_probe":
                        packet.Flow_probe = GetNullableDecimal(ref reader);
                        break;
                    case "temperature_probe":
                        packet.Temperature_probe = GetNullableDecimal(ref reader);
                        break;
                    case "humidity_probe":
                        packet.Humidity_probe = GetNullableDecimal(ref reader);
                        break;
                    case "laser_status":
                        packet.Laser_status = GetNullableInt(ref reader);
                        break;
                    case "supply_voltage":
                        packet.Supply_voltage = GetNullableDecimal(ref reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }
        
        throw new JsonException("Unexpected end when reading DustPacket");
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
    
    public override void Write(Utf8JsonWriter writer, DustPacket value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}