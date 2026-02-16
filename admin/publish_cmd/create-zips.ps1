param(
    [ValidateSet("linux-arm64", "linux-x64", "win-x64", "all")]
    [string]$Runtime = "all"
)

$basePath = "C:\git\BurstroyMonitoring"
$runtimes = @("linux-arm64", "linux-x64", "win-x64")

# Если выбрана конкретная runtime, используем только ее
if ($Runtime -ne "all") {
    $runtimes = @($Runtime)
}

foreach ($rt in $runtimes) {
    # TCM
    $tcmPath = "$basePath\BurstroyMonitoring.TCM\release\$rt"
    $tcmFiles = @()
    
    if (Test-Path $tcmPath) {
        $wwwrootPath = "$tcmPath\wwwroot"
        
        # Добавляем файлы
        if (Test-Path "$tcmPath\BurstroyMonitoring.TCM") {
            $tcmFiles += "$tcmPath\BurstroyMonitoring.TCM"
        }
        if (Test-Path "$tcmPath\appsettings.json") {
            $tcmFiles += "$tcmPath\appsettings.json"
        }
        if (Test-Path $wwwrootPath) {
            $tcmFiles += $wwwrootPath
        }
        
        # Создаем архив, если есть файлы
        if ($tcmFiles.Count -gt 0) {
            $zipPath = "$basePath\BurstroyMonitoring.TCM\release\$runtimes\BurstroyMonitoring.TCM.zip"
            Compress-Archive -Path $tcmFiles -DestinationPath $zipPath -Force
            Write-Host "Created: $zipPath" -ForegroundColor Green
        }
    }
    
    # Worker
    $workerPath = "$basePath\BurstroyMonitoring.Worker\release\$rt"
    $workerFiles = @()
    
    if (Test-Path $workerPath) {
        # Добавляем файлы
        if (Test-Path "$workerPath\BurstroyMonitoring.Worker") {
            $workerFiles += "$workerPath\BurstroyMonitoring.Worker"
        }
        if (Test-Path "$workerPath\appsettings.json") {
            $workerFiles += "$workerPath\appsettings.json"
        }
        
        # Создаем архив, если есть файлы
        if ($workerFiles.Count -gt 0) {
            $zipPath = "$basePath\BurstroyMonitoring.Worker\release\$runtimes\BurstroyMonitoring.Worker.zip"
            Compress-Archive -Path $workerFiles -DestinationPath $zipPath -Force
            Write-Host "Created: $zipPath" -ForegroundColor Green
        }
    }
}

Write-Host "`nArchiving completed!" -ForegroundColor Cyan