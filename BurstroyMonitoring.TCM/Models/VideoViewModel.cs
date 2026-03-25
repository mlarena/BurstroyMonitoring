namespace BurstroyMonitoring.TCM.Models;

public class VideoViewModel
{
    public string RtspUrl { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public string? ErrorMessage { get; set; }
}
