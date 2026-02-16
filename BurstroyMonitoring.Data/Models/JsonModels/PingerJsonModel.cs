using System.Text.Json;
using System.Text.Json.Serialization;

namespace BurstroyMonitoring.Data.Models.JsonModels;

/// <summary>
/// Модель для десериализации JSON от датчиков MUEKS
/// </summary>
public class MueksJsonModel
{
    [JsonPropertyName("Serial")]
    public string? Serial { get; set; }
    
    [JsonPropertyName("Packet")]
    [JsonConverter(typeof(MueksPacketConverter))]
    public MueksPacket? Packet { get; set; }
}

public class MueksPacket
{
    public string? DataTime { get; set; }
    public decimal? TemperatureBox { get; set; }
    public decimal? UPowerIn12B { get; set; }
    public decimal? UOut12B { get; set; }
    public decimal? IOut12B { get; set; }
    public decimal? IOut48B { get; set; }
    public decimal? UAkb { get; set; }
    public decimal? IAkb { get; set; }
    public int? Sens220B { get; set; }
    public decimal? WhAkb { get; set; }
    public decimal? VisibleRange { get; set; }
    public int? DoorStatus { get; set; }
    public string? TdsH { get; set; }
    public string? TdsTds { get; set; }
    public string? TkosaT1 { get; set; }
    public string? TkosaT3 { get; set; }
}

public class MueksPacketConverter : JsonConverter<MueksPacket>
{
    public override MueksPacket Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var packet = new MueksPacket();
        
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
                    case "temperature_box":
                        packet.TemperatureBox = GetNullableDecimal(ref reader);
                        break;
                    case "u_power_in_12b":
                        packet.UPowerIn12B = GetNullableDecimal(ref reader);
                        break;
                    case "u_out_12b":
                        packet.UOut12B = GetNullableDecimal(ref reader);
                        break;
                    case "i_out_12b":
                        packet.IOut12B = GetNullableDecimal(ref reader);
                        break;
                    case "i_out_48b":
                        packet.IOut48B = GetNullableDecimal(ref reader);
                        break;
                    case "u_akb":
                        packet.UAkb = GetNullableDecimal(ref reader);
                        break;
                    case "i_akb":
                        packet.IAkb = GetNullableDecimal(ref reader);
                        break;
                    case "sens_220b":
                        packet.Sens220B = GetNullableInt(ref reader);
                        break;
                    case "wh_akb":
                        packet.WhAkb = GetNullableDecimal(ref reader);
                        break;
                    case "visible_range":
                        packet.VisibleRange = GetNullableDecimal(ref reader);
                        break;
                    case "door_status":
                        packet.DoorStatus = GetNullableInt(ref reader);
                        break;
                    case "tds_h":
                        packet.TdsH = reader.GetString();
                        break;
                    case "tds_tds":
                        packet.TdsTds = reader.GetString();
                        break;
                    case "tkosa_t1":
                        packet.TkosaT1 = reader.GetString();
                        break;
                    case "tkosa_t3":
                        packet.TkosaT3 = reader.GetString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }
        
        throw new JsonException("Unexpected end when reading MueksPacket");
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
    
    public override void Write(Utf8JsonWriter writer, MueksPacket value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}