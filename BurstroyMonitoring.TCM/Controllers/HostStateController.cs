using Microsoft.AspNetCore.Mvc;
using BurstroyMonitoring.TCM.Models;
using BurstroyMonitoring.TCM.Services;
using System.Diagnostics;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class HostStateController : Controller
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<HostStateController> _logger;

        public HostStateController(IDatabaseService databaseService, ILogger<HostStateController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HostStateModel
            {
                CheckedAt = DateTime.Now,
                // Получаем имя БД из строки подключения
                DatabaseName = _databaseService.GetDatabaseName(),
                DiskInfoList = new List<DiskInfo>()
            };

            // Получение размера БД
            try
            {
                var sizeInMB = await _databaseService.GetDatabaseSizeInMB();
                model.DatabaseSizeMB = Math.Round(sizeInMB, 2);
                model.DatabaseSizeFormatted = FormatSize(sizeInMB);
                model.IsConnected = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении размера БД");
                model.IsConnected = false;
                model.ErrorMessage = ex.Message;
                model.DatabaseSizeMB = 0;
                model.DatabaseSizeFormatted = "0 MB";
            }

            // Получение информации df -h
            try
            {
                model.DiskInfoList = GetDiskUsageInfo();
                model.DiskUsageInfo = GetDiskUsageAsString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении df -h");
                model.DiskUsageInfo = $"Ошибка: {ex.Message}";
            }

            return View(model);
        }

        private string FormatSize(double sizeInMB)
        {
            if (sizeInMB >= 1024)
            {
                return $"{Math.Round(sizeInMB / 1024, 2)} GB";
            }
            return $"{Math.Round(sizeInMB, 2)} MB";
        }

        private List<DiskInfo> GetDiskUsageInfo()
        {
            var diskInfoList = new List<DiskInfo>();
            
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "df",
                    Arguments = "-h",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = lines[i].Trim();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 6)
                        {
                            diskInfoList.Add(new DiskInfo
                            {
                                Filesystem = parts[0],
                                Size = parts[1],
                                Used = parts[2],
                                Available = parts[3],
                                UsePercent = parts[4],
                                MountedOn = parts[5]
                            });
                        }
                    }
                }
                else
                {
                    throw new Exception($"Ошибка выполнения df -h: {error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о дисках");
                throw;
            }

            return diskInfoList;
        }

        private string GetDiskUsageAsString()
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "df",
                    Arguments = "-h",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output;
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }
    }
}