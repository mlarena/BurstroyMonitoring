$paths = @(
    "C:\git\BurstroyMonitoring\BurstroyMonitoring.TCM\release\linux-arm64",
    "C:\git\BurstroyMonitoring\BurstroyMonitoring.TCM\release\linux-x64",
    "C:\git\BurstroyMonitoring\BurstroyMonitoring.TCM\release\win-x64",
    "C:\git\BurstroyMonitoring\BurstroyMonitoring.Worker\release\linux-arm64",
    "C:\git\BurstroyMonitoring\BurstroyMonitoring.Worker\release\linux-x64",
    "C:\git\BurstroyMonitoring\BurstroyMonitoring.Worker\release\win-x64"
)

$paths | ForEach-Object {
    if (Test-Path $_) {
        Remove-Item "$_\*" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "clean: $_" -ForegroundColor Green
    }
}