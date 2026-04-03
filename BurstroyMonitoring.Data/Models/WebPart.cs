using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BurstroyMonitoring.Data.Models;

[Table("web_parts")]
public class WebPart
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("type")]
    public WebPartType Type { get; set; }

    [Column("position_x")]
    public int PositionX { get; set; }

    [Column("position_y")]
    public int PositionY { get; set; }

    [Column("width")]
    public int Width { get; set; } = 6;

    [Column("height")]
    public int Height { get; set; } = 4;

    [Column("settings")]
    public string Settings { get; set; } = "{}";

    [Column("data")]
    public string Data { get; set; } = "{}";

    [Column("user_id")]
    public int UserId { get; set; }
}
