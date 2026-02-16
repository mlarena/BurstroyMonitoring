param(
    [string]$Project = "TCM",  # TCM or Worker
    [ValidateSet("linux-x64", "linux-arm64", "win-x64")]
    [string]$Runtime = "linux-x64"
)

# Determine project path based on parameter
$basePath = "C:\git\BurstroyMonitoring"

if ($Project -eq "TCM") {
    $projectPath = "$basePath\BurstroyMonitoring.TCM\BurstroyMonitoring.TCM.csproj"
    $outputPath = "$basePath\BurstroyMonitoring.TCM\release\$Runtime\"
} elseif ($Project -eq "Worker") {
    $projectPath = "$basePath\BurstroyMonitoring.Worker\BurstroyMonitoring.Worker.csproj"
    $outputPath = "$basePath\BurstroyMonitoring.Worker\release\$Runtime\"
} else {
    Write-Host "Unknown project. Use TCM or Worker" -ForegroundColor Red
    exit 1
}

Write-Host "Publishing project: $Project" -ForegroundColor Yellow
Write-Host "Runtime: $Runtime" -ForegroundColor Yellow
Write-Host "Output folder: $outputPath" -ForegroundColor Yellow
Write-Host "-" * 50

# Create output directory if it doesn't exist
New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

# Execute publish command
dotnet publish $projectPath `
    -c Release `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -o $outputPath

# Check the result
if ($LASTEXITCODE -eq 0) {
    Write-Host "`nSuccess! $Project published for $Runtime." -ForegroundColor Green
    Write-Host "Files saved to: $outputPath" -ForegroundColor Cyan
} else {
    Write-Host "`nError during publishing!" -ForegroundColor Red
    exit 1
}