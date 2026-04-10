# QuickEncodingCheck.ps1
Get-ChildItem -Recurse -File | ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    
    # Detect encoding
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $encoding = "UTF-8 with BOM"
    }
    elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        $encoding = "UTF-16 LE"
    }
    elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
        $encoding = "UTF-16 BE"
    }
    else {
        try {
            [System.Text.UTF8Encoding]::new($false, $true).GetString($bytes) | Out-Null
            $encoding = "UTF-8 without BOM"
        }
        catch {
            $encoding = "Other (likely ANSI)"
        }
    }
    
    [PSCustomObject]@{
        File = $_.FullName
        Encoding = $encoding
    }
} | Format-Table -AutoSize