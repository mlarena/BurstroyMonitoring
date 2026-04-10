$paths = @(
    "C:\git\BurstroyMonitoring\BurstroyMonitoring.TCM\release",
    "C:\git\BurstroyMonitoring\BurstroyMonitoring.Worker\release",
    "C:\git\burstroymonitoring\BurstroyMonitoring.VideoMonitoring\release",
    "C:\git\burstroymonitoring\BurstroyMonitoring.Api\release"    
)

$paths | ForEach-Object {
    if (Test-Path $_) {
        Remove-Item "$_\*" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "clean: $_" -ForegroundColor Green
    }
}