using Microsoft.EntityFrameworkCore;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using System.Diagnostics;

namespace BurstroyMonitoring.VideoMonitoring;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;
    private readonly string _baseDirectory;

    public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _baseDirectory = configuration["SnapshotSettings:BaseDirectory"] ?? "wwwroot/snapshots";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var cameras = await context.Cameras.ToListAsync(stoppingToken);

                    foreach (var camera in cameras)
                    {
                        await ProcessCameraAsync(camera, context, stoppingToken);
                    }
                    
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker loop");
            }

            await Task.Delay(10000, stoppingToken);
        }
    }

    private async Task ProcessCameraAsync(Camera camera, ApplicationDbContext context, CancellationToken stoppingToken)
    {
        var lastSnapshot = await context.Snapshots
            .Where(s => s.CameraId == camera.Id)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(stoppingToken);

        if (lastSnapshot != null && (DateTime.UtcNow - lastSnapshot.CreatedAt).TotalSeconds < camera.PollingInterval)
        {
            return;
        }

        _logger.LogInformation("Taking snapshot for camera: {CameraName}", camera.Name);

        string fullUrl = camera.RtspUrl;
        if (!string.IsNullOrEmpty(camera.Username) && !camera.RtspUrl.Contains("@"))
        {
            fullUrl = camera.RtspUrl.Replace("://", $"://{camera.Username}:{camera.Password}@");
        }

        var fileName = $"{Guid.NewGuid()}.jpg";
        var relativePath = Path.Combine(camera.Id.ToString(), fileName);
        var absolutePath = Path.Combine(_baseDirectory, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        bool success = await TakeFfmpegSnapshotAsync(fullUrl, absolutePath, stoppingToken);

        if (success)
        {
            var snapshot = new Snapshot
            {
                CameraId = camera.Id,
                FilePath = "/snapshots/" + relativePath.Replace("\\", "/"),
                CreatedAt = DateTime.UtcNow
            };
            context.Snapshots.Add(snapshot);
            _logger.LogInformation("Snapshot saved for camera: {CameraName}", camera.Name);
        }
        else
        {
            _logger.LogWarning("Failed to take snapshot for camera: {CameraName}", camera.Name);
        }
    }

    private async Task<bool> TakeFfmpegSnapshotAsync(string rtspUrl, string outputPath, CancellationToken stoppingToken)
    {
        var arguments = $"-rtsp_transport tcp -i \"{rtspUrl}\" -frames:v 1 -q:v 2 \"{outputPath}\" -y";
        
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(stoppingToken);
            return process.ExitCode == 0;
        }
        catch (Exception)
        {
            return false;
        }
    }}
