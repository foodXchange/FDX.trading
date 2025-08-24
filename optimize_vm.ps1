# VM Optimization Script for FDX Development
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VM OPTIMIZATION SCRIPT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. Clean Temp Files
Write-Host "`n[1/6] Cleaning temporary files..." -ForegroundColor Yellow
$tempFolders = @(
    $env:TEMP,
    "$env:WINDIR\Temp",
    "$env:LOCALAPPDATA\Temp"
)

foreach ($folder in $tempFolders) {
    if (Test-Path $folder) {
        $sizeBefore = (Get-ChildItem $folder -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        Remove-Item "$folder\*" -Recurse -Force -ErrorAction SilentlyContinue
        $sizeAfter = (Get-ChildItem $folder -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        $freed = [math]::Round($sizeBefore - $sizeAfter, 2)
        Write-Host "  Cleaned $folder - Freed: $freed MB" -ForegroundColor Green
    }
}

# 2. Clear NuGet Cache
Write-Host "`n[2/6] Clearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear 2>$null
Write-Host "  NuGet cache cleared" -ForegroundColor Green

# 3. Clean Visual Studio Temp Files
Write-Host "`n[3/6] Cleaning Visual Studio temporary files..." -ForegroundColor Yellow
$vsTemp = "$env:LOCALAPPDATA\Microsoft\VisualStudio"
if (Test-Path $vsTemp) {
    Get-ChildItem "$vsTemp\*\ComponentModelCache" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  VS Component Model Cache cleared" -ForegroundColor Green
}

# 4. Close unnecessary Edge processes (keep only 2)
Write-Host "`n[4/6] Optimizing Edge browser processes..." -ForegroundColor Yellow
$edgeProcesses = Get-Process msedge -ErrorAction SilentlyContinue | Sort-Object WorkingSet64 -Descending
if ($edgeProcesses.Count -gt 2) {
    $toClose = $edgeProcesses | Select-Object -Skip 2
    $toClose | Stop-Process -Force -ErrorAction SilentlyContinue
    Write-Host "  Closed $($toClose.Count) excess Edge processes" -ForegroundColor Green
}

# 5. Disable Windows Search if not needed
Write-Host "`n[5/6] Checking Windows Search service..." -ForegroundColor Yellow
$searchService = Get-Service WSearch -ErrorAction SilentlyContinue
if ($searchService.Status -eq 'Running') {
    Write-Host "  Windows Search is running. Consider disabling if not needed:" -ForegroundColor Yellow
    Write-Host "  Run: Stop-Service WSearch -Force" -ForegroundColor Gray
}

# 6. Show memory status
Write-Host "`n[6/6] Current Memory Status:" -ForegroundColor Yellow
$os = Get-CimInstance Win32_OperatingSystem
$totalMemory = [math]::Round($os.TotalVisibleMemorySize / 1MB, 2)
$freeMemory = [math]::Round($os.FreePhysicalMemory / 1MB, 2)
$usedMemory = $totalMemory - $freeMemory
$percentUsed = [math]::Round(($usedMemory / $totalMemory) * 100, 2)

Write-Host "  Total RAM: $totalMemory GB" -ForegroundColor Cyan
Write-Host "  Used RAM: $usedMemory GB ($percentUsed%)" -ForegroundColor $(if ($percentUsed -gt 80) { "Red" } elseif ($percentUsed -gt 60) { "Yellow" } else { "Green" })
Write-Host "  Free RAM: $freeMemory GB" -ForegroundColor Green

# Show top memory consumers
Write-Host "`nTop 5 Memory Consumers:" -ForegroundColor Yellow
Get-Process | Sort-Object WorkingSet64 -Descending | Select-Object -First 5 | ForEach-Object {
    $memMB = [math]::Round($_.WorkingSet64 / 1MB, 2)
    Write-Host "  $($_.Name): $memMB MB" -ForegroundColor Gray
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "OPTIMIZATION COMPLETE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan