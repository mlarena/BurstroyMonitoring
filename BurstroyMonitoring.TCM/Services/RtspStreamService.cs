using System.Diagnostics;
using Serilog;

namespace BurstroyMonitoring.TCM.Services;

public class RtspStreamService : IDisposable
{
    private Process? _ffmpegProcess;
    private MemoryStream? _currentFrame;
    private readonly object _lock = new();
    private bool _isRunning;
    private DateTime _lastFrameTime = DateTime.MinValue;
    private readonly Serilog.ILogger _logger = Log.ForContext<RtspStreamService>();

    public string? LastError { get; private set; }
    public bool IsRunning => _isRunning;

    public void Start(string rtspUrl)
    {
        if (_isRunning) return;

        LastError = null;
        
        // Очищаем старый кадр перед запуском нового потока
        lock (_lock)
        {
            _currentFrame?.Dispose();
            _currentFrame = null;
            _lastFrameTime = DateTime.MinValue;
        }

        _logger.Information("[RTSP] Подключение к {Url}...", rtspUrl);

        var arguments = string.Join(" ", new[]
        {
            "-rtsp_transport tcp",
            "-analyzeduration 10M",      // Анализируем 10 секунд
            "-probesize 10M",            // Размер буфера анализа
            "-fflags nobuffer",
            "-flags low_delay",
            "-i", $"\"{rtspUrl}\"",
            "-vf", "\"fps=10,scale=640:-1,format=yuv420p\"",
            "-f", "image2pipe",
            "-vcodec", "mjpeg",
            "-q:v", "2",
            "pipe:1"
        });

        _logger.Debug("[FFmpeg] {Arguments}", arguments);

        _ffmpegProcess = new Process
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

        _ffmpegProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _logger.Information("[FFmpeg] {Data}", e.Data);
                if (e.Data.Contains("error", StringComparison.OrdinalIgnoreCase) || 
                    e.Data.Contains("failed", StringComparison.OrdinalIgnoreCase))
                    LastError = e.Data;
            }
        };

        _ffmpegProcess.Start();
        _ffmpegProcess.BeginErrorReadLine();
        
        _isRunning = true;
        Task.Run(ReadFramesAsync);
    }

    private async Task ReadFramesAsync()
    {
        if (_ffmpegProcess?.StandardOutput.BaseStream == null) return;

        var stream = _ffmpegProcess.StandardOutput.BaseStream;
        var buffer = new byte[262144]; // Увеличили буфер для HD видео
        var frameBuffer = new List<byte>();
        int emptyReads = 0;
        int frameCount = 0;

        while (_isRunning && !_ffmpegProcess.HasExited)
        {
            try
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                
                if (bytesRead == 0)
                {
                    emptyReads++;
                    if (emptyReads > 30) break;
                    await Task.Delay(100);
                    continue;
                }
                
                emptyReads = 0;
                frameBuffer.AddRange(buffer.Take(bytesRead));

                // Обрабатываем все кадры в буфере
                while (true)
                {
                    var data = frameBuffer.ToArray();
                    int start = FindMarker(data, new byte[] { 0xFF, 0xD8 });
                    if (start < 0) break;
                    
                    int end = FindMarker(data, new byte[] { 0xFF, 0xD9 }, start + 2);
                    if (end < 0) break;

                    var jpegData = data.Skip(start).Take(end - start + 2).ToArray();
                    
                    lock (_lock)
                    {
                        _currentFrame = new MemoryStream(jpegData);
                        _lastFrameTime = DateTime.Now;
                    }
                    
                    frameCount++;
                    if (frameCount % 30 == 0)
                        _logger.Information("[RTSP] Получено кадров: {Count}", frameCount);
                    
                    frameBuffer = frameBuffer.Skip(end + 2).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[RTSP] Ошибка при чтении кадров");
                break;
            }
        }
        
        _isRunning = false;
        _logger.Information("[RTSP] Поток завершён. Всего кадров: {Count}", frameCount);
    }

    private int FindMarker(byte[] data, byte[] marker, int startIndex = 0)
    {
        for (int i = startIndex; i <= data.Length - marker.Length; i++)
            if (data[i] == marker[0] && data[i + 1] == marker[1])
                return i;
        return -1;
    }

    public byte[]? GetFrame()
    {
        lock (_lock)
        {
            // Увеличили таймаут до 5 секунд для медленного старта
            if (_currentFrame == null || (DateTime.Now - _lastFrameTime).TotalSeconds > 5)
                return null;
                
            return _currentFrame.ToArray();
        }
    }

    public void Dispose()
    {
        _isRunning = false;
        try { _ffmpegProcess?.Kill(); } catch { }
        _ffmpegProcess?.Dispose();
        _currentFrame?.Dispose();
    }
}
