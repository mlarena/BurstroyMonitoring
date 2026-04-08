namespace BurstroyMonitoring.TCM.Models
{
    public class HostStateModel
    {
        public double DatabaseSizeMB { get; set; }
        public string DatabaseSizeFormatted { get; set; }
        public string DatabaseName { get; set; }
        public DateTime CheckedAt { get; set; }
        public bool IsConnected { get; set; }
        public string ErrorMessage { get; set; }
        
        // Новые свойства для df -h
        public string DiskUsageInfo { get; set; }
        public List<DiskInfo> DiskInfoList { get; set; }
    }

    public class DiskInfo
    {
        public string Filesystem { get; set; }
        public string Size { get; set; }
        public string Used { get; set; }
        public string Available { get; set; }
        public string UsePercent { get; set; }
        public string MountedOn { get; set; }
    }
}