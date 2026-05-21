# Debug Launch Script for PDI_SiteDataSync
# This script helps verify file archival during debugging

Write-Host "=== PDI_SiteDataSync Debug Helper ===" -ForegroundColor Cyan
Write-Host ""

# Read configuration
$configPath = "PDI_SiteDataSync\appsettings.json"
$config = Get-Content $configPath | ConvertFrom-Json

$baseFolder = $config.FolderSettings.BaseFolder
$inputFolder = Join-Path $baseFolder $config.FolderSettings.InputSubfolder
$logsFolder = Join-Path $baseFolder $config.FolderSettings.LogsSubfolder
$archiveFolder = Join-Path $baseFolder $config.FolderSettings.ArchiveSubfolder

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Base Folder:    $baseFolder"
Write-Host "  Input Folder:   $inputFolder"
Write-Host "  Logs Folder:    $logsFolder"
Write-Host "  Archive Folder: $archiveFolder"
Write-Host ""

# Check if folders exist and create them
Write-Host "Ensuring folders exist..." -ForegroundColor Yellow
@($baseFolder, $inputFolder, $logsFolder, $archiveFolder) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
        Write-Host "  Created: $_" -ForegroundColor Green
    } else {
        Write-Host "  Exists:  $_" -ForegroundColor Gray
    }
}
Write-Host ""

# Check for Excel files in Input folder
Write-Host "Checking for Excel files in Input folder..." -ForegroundColor Yellow
$excelFiles = Get-ChildItem -Path $inputFolder -Filter "*.xlsx" -ErrorAction SilentlyContinue
if ($excelFiles.Count -eq 0) {
    Write-Host "  WARNING: No Excel files found in Input folder!" -ForegroundColor Red
    Write-Host "  Please place an Excel file with 'Store List' worksheet in: $inputFolder" -ForegroundColor Red
} else {
    Write-Host "  Found $($excelFiles.Count) Excel file(s):" -ForegroundColor Green
    $excelFiles | ForEach-Object {
        Write-Host "    - $($_.Name) ($('{0:N2}' -f ($_.Length/1KB)) KB)" -ForegroundColor Gray
    }
}
Write-Host ""

# Capture state before run
$inputFilesBefore = @(Get-ChildItem -Path $inputFolder -Filter "*.xlsx" -ErrorAction SilentlyContinue)
$archiveFilesBefore = @(Get-ChildItem -Path $archiveFolder -Filter "Data_*.zip" -ErrorAction SilentlyContinue)

Write-Host "=== Starting Application in Debug Mode ===" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to cancel, or let it run to completion..." -ForegroundColor Yellow
Write-Host ""

# Launch in debug mode
$exePath = "PDI_SiteDataSync\bin\Debug\net10.0\PDI_SiteDataSync.exe"
if (Test-Path $exePath) {
    & $exePath
    $exitCode = $LASTEXITCODE
    Write-Host ""
    Write-Host "=== Application Completed (Exit Code: $exitCode) ===" -ForegroundColor Cyan
} else {
    Write-Host "ERROR: Executable not found: $exePath" -ForegroundColor Red
    Write-Host "Run: dotnet build PDI_SiteDataSync/PDI_SiteDataSync.csproj --configuration Debug" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "=== Verification ===" -ForegroundColor Cyan
Write-Host ""

# Check if input file was archived
$inputFilesAfter = @(Get-ChildItem -Path $inputFolder -Filter "*.xlsx" -ErrorAction SilentlyContinue)
$archiveFilesAfter = @(Get-ChildItem -Path $archiveFolder -Filter "Data_*.zip" -ErrorAction SilentlyContinue)

Write-Host "Input Folder Status:" -ForegroundColor Yellow
Write-Host "  Files before: $($inputFilesBefore.Count)"
Write-Host "  Files after:  $($inputFilesAfter.Count)"

if ($inputFilesAfter.Count -lt $inputFilesBefore.Count) {
    Write-Host "  ✓ Input file(s) were REMOVED (archived)" -ForegroundColor Green
} elseif ($inputFilesBefore.Count -gt 0) {
    Write-Host "  ✗ WARNING: Input file(s) still present!" -ForegroundColor Red
    Write-Host "    Expected files to be deleted after archival." -ForegroundColor Red
} else {
    Write-Host "  (No input files to process)" -ForegroundColor Gray
}
Write-Host ""

Write-Host "Archive Folder Status:" -ForegroundColor Yellow
Write-Host "  Data archives before: $($archiveFilesBefore.Count)"
Write-Host "  Data archives after:  $($archiveFilesAfter.Count)"

if ($archiveFilesAfter.Count -gt $archiveFilesBefore.Count) {
    Write-Host "  ✓ NEW archive file(s) created!" -ForegroundColor Green
    $newArchives = $archiveFilesAfter | Where-Object { $_.Name -notin $archiveFilesBefore.Name }
    $newArchives | ForEach-Object {
        Write-Host "    - $($_.Name) ($('{0:N2}' -f ($_.Length/1KB)) KB)" -ForegroundColor Green

        # Try to list contents of the zip
        try {
            Add-Type -AssemblyName System.IO.Compression.FileSystem
            $zip = [System.IO.Compression.ZipFile]::OpenRead($_.FullName)
            Write-Host "      Contents:" -ForegroundColor Gray
            $zip.Entries | ForEach-Object {
                Write-Host "        + $($_.Name)" -ForegroundColor Gray
            }
            $zip.Dispose()
        } catch {
            Write-Host "      (Unable to read zip contents)" -ForegroundColor DarkGray
        }
    }
} elseif ($inputFilesBefore.Count -gt 0) {
    Write-Host "  ✗ WARNING: No new archive files created!" -ForegroundColor Red
    Write-Host "    Check logs for archival errors." -ForegroundColor Red
} else {
    Write-Host "  (No input files to archive)" -ForegroundColor Gray
}
Write-Host ""

# Check log files
$logFiles = Get-ChildItem -Path $logsFolder -Filter "*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
if ($logFiles.Count -gt 0) {
    Write-Host "Recent Log File:" -ForegroundColor Yellow
    $latestLog = $logFiles[0]
    Write-Host "  $($latestLog.Name) (Last modified: $($latestLog.LastWriteTime))" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Last 20 lines of log:" -ForegroundColor Yellow
    Get-Content $latestLog.FullName -Tail 20 | ForEach-Object {
        if ($_ -match "ERROR") {
            Write-Host "  $_" -ForegroundColor Red
        } elseif ($_ -match "WARN") {
            Write-Host "  $_" -ForegroundColor Yellow
        } elseif ($_ -match "archive|Archive") {
            Write-Host "  $_" -ForegroundColor Green
        } else {
            Write-Host "  $_" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "=== Debug Session Complete ===" -ForegroundColor Cyan
