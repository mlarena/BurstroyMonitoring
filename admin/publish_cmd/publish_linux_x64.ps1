# Publish and Zip script for Linux x64
$ErrorActionPreference = "Stop"

$basePath = "C:\git\BurstroyMonitoring"
$runtime = "linux-x64"
$publishDirName = "release\$runtime"

$projects = @(
    @{ Name = "BurstroyMonitoring.TCM"; Path = "$basePath\BurstroyMonitoring.TCM\BurstroyMonitoring.TCM.csproj"; Out = "$basePath\BurstroyMonitoring.TCM\$publishDirName" },
    @{ Name = "BurstroyMonitoring.Worker"; Path = "$basePath\BurstroyMonitoring.Worker\BurstroyMonitoring.Worker.csproj"; Out = "$basePath\BurstroyMonitoring.Worker\$publishDirName" },
    @{ Name = "BurstroyMonitoring.VideoMonitoring"; Path = "$basePath\BurstroyMonitoring.VideoMonitoring\BurstroyMonitoring.VideoMonitoring.csproj"; Out = "$basePath\BurstroyMonitoring.VideoMonitoring\$publishDirName" }
)

foreach ($project in $projects) {
    Write-Host "--- Building $($project.Name) for $runtime ---" -ForegroundColor Cyan
    
    # 1. Publish
    dotnet publish $project.Path -c Release -r $runtime --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $project.Out
    
    # 2. Create ZIP
    $zipPath = Join-Path $project.Out "$($project.Name).zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath }
    
    Write-Host "Archiving to $($project.Name).zip..." -ForegroundColor Yellow
    
    Push-Location $project.Out
    try {
        $filesToZip = @()
        if (Test-Path $project.Name) { $filesToZip += $project.Name }
        if (Test-Path "appsettings.json") { $filesToZip += "appsettings.json" }
        if (Test-Path "wwwroot") { $filesToZip += "wwwroot" }

        Add-Type -AssemblyName "System.IO.Compression.FileSystem"
        $zipArchive = [System.IO.Compression.ZipFile]::Open($zipPath, "Create")
        
        foreach ($item in $filesToZip) {
            if (Test-Path $item -PathType Container) {
                $files = Get-ChildItem $item -Recurse
                foreach ($file in $files) {
                    if (-not $file.PSIsContainer) {
                        $relativeName = $file.FullName.Substring($project.Out.Length + 1).Replace('\', '/')
                        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipArchive, $file.FullName, $relativeName)
                    }
                }
            } else {
                $relativeName = $item.Replace('\', '/')
                [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipArchive, (Join-Path $project.Out $item), $relativeName)
            }
        }
        $zipArchive.Dispose()
    }
    finally {
        Pop-Location
    }
    Write-Host "Done: $zipPath" -ForegroundColor Green
}
