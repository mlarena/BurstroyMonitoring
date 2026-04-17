# Script to create a deployment bundle for Linux x64
$ErrorActionPreference = "Stop"

$basePath = "C:\git\BurstroyMonitoring"
$outputDir = "$basePath\admin\publish_cmd"
$zipPath = Join-Path $outputDir "tcm_arm.zip"

# List of files to include in the bundle
$filesToInclude = @(
    # Setup scripts
    "$basePath\admin\bash\setup_dotnet.sh",
    "$basePath\admin\bash\setup_nginx.sh",
    "$basePath\admin\bash\setup_postgresql.sh",
    "$basePath\admin\bash\setup-nginx-proxy.sh",
    "$basePath\admin\bash\check-dependencies.sh"
    
    # Application binaries (Linux x64)
    "$basePath\BurstroyMonitoring.VideoMonitoring\release\linux-arm64\BurstroyMonitoring.VideoMonitoring.zip",
    "$basePath\BurstroyMonitoring.Worker\release\linux-arm64\BurstroyMonitoring.Worker.zip",
    "$basePath\BurstroyMonitoring.TCM\release\linux-arm64\BurstroyMonitoring.TCM.zip",
    "$basePath\BurstroyMonitoring.Api\release\linux-arm64\BurstroyMonitoring.Api.zip"
    
    # SQL structure
    "$basePath\admin\sql\create_database.sql",
    "$basePath\admin\sql\db_insert_internal_test_MonitoringPost.sql",
    "$basePath\admin\sql\db_insert_sensors_default.sql",


    # Service creation scripts
    "$basePath\admin\bash\create-service-burstroy-monitoring-tcm.sh",
    "$basePath\admin\bash\create-service-burstroy-monitoring-video.sh",
    "$basePath\admin\bash\create-service-burstroy-monitoring-worker.sh",
    "$basePath\admin\bash\create-service-burstroy-monitoring-api.sh",
    
    # Installation scripts
    "$basePath\admin\bash\install_tcm.sh",
    "$basePath\admin\bash\install_video-monitoring.sh",
    "$basePath\admin\bash\install_worker.sh",
    "$basePath\admin\bash\install_api.sh",
    
    "$basePath\admin\bash\1_install_system.sh",
    "$basePath\admin\bash\2_update_system.sh",
    "$basePath\admin\bash\3_start_services.sh",
    "$basePath\admin\bash\4_status_services.sh",
    "$basePath\admin\bash\5_stop_services.sh",
    
    # Update scripts
    "$basePath\admin\bash\update_tcm.sh",
    "$basePath\admin\bash\update_video-monitoring.sh",
    "$basePath\admin\bash\update_worker.sh",
    "$basePath\admin\bash\update_api.sh"
)

Write-Host "Creating deployment bundle: $zipPath" -ForegroundColor Cyan

# Remove old zip if exists
if (Test-Path $zipPath) { Remove-Item $zipPath }

# Use .NET ZipFile for creation
Add-Type -AssemblyName "System.IO.Compression.FileSystem"
$zipArchive = [System.IO.Compression.ZipFile]::Open($zipPath, "Create")

try {
    foreach ($filePath in $filesToInclude) {
        if (Test-Path $filePath) {
            $fileName = Split-Path $filePath -Leaf
            Write-Host "Adding: $fileName" -ForegroundColor Yellow
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipArchive, $filePath, $fileName)
        } else {
            Write-Warning "File not found, skipping: $filePath"
        }
    }
}
finally {
    $zipArchive.Dispose()
}

Write-Host "`nSuccessfully created: $zipPath" -ForegroundColor Green
Write-Host "You can now copy this file to the server using SCP." -ForegroundColor Gray
