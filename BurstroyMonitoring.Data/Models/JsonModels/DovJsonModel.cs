using System.Text.Json;
using System.Text.Json.Serialization;

namespace BurstroyMonitoring.Data.Models.JsonModels;



/// <summary>
/// Модель для десериализации JSON от датчиков DOV
/// </summary>
public class DovJsonModel
{
    [JsonPropertyName("Serial")]
    public string? Serial { get; set; }
    
    [JsonPropertyName("Packet")]
    [JsonConverter(typeof(DovPacketConverter))]
    public DovPacket? Packet { get; set; }
}

public class DovPacket
{
    public decimal? VisibleRange { get; set; }
    public int? BrightFlag { get; set; }
}

public class DovPacketConverter : JsonConverter<DovPacket>
{
    public override DovPacket Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var packet = new DovPacket();
        
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
                    case "visible_range":
                        packet.VisibleRange = GetNullableDecimal(ref reader);
                        break;
                    case "bright_flag":
                        packet.BrightFlag = GetNullableInt(ref reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }
        
        throw new JsonException("Unexpected end when reading DovPacket");
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
    
    public override void Write(Utf8JsonWriter writer, DovPacket value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}