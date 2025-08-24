# System Performance Optimization Script
# Date: January 20, 2025
# Purpose: Optimize Windows Server and IIS for FoodX Application

Write-Host "Starting System Performance Optimization..." -ForegroundColor Green

# 1. Optimize IIS Application Pool Settings
Write-Host "`n1. Optimizing IIS Application Pool..." -ForegroundColor Yellow
Import-Module WebAdministration -ErrorAction SilentlyContinue

$appPoolName = "FoodXAdminPool"
$siteName = "FoodXAdmin"

# Check if IIS is installed
if (Get-Module -ListAvailable -Name WebAdministration) {
    # Create optimized application pool if it doesn't exist
    if (!(Test-Path "IIS:\AppPools\$appPoolName")) {
        New-WebAppPool -Name $appPoolName
    }

    # Configure application pool for optimal performance
    $appPool = Get-Item "IIS:\AppPools\$appPoolName"
    $appPool.processModel.identityType = "ApplicationPoolIdentity"
    $appPool.recycling.periodicRestart.time = [TimeSpan]::Zero
    $appPool.recycling.periodicRestart.requests = 0
    $appPool.processModel.idleTimeout = [TimeSpan]::FromMinutes(0)
    $appPool.processModel.maxProcesses = 1
    $appPool.startMode = "AlwaysRunning"
    $appPool.enable32BitAppOnWin64 = $false
    $appPool.managedRuntimeVersion = ""  # No Managed Code for .NET Core
    $appPool | Set-Item

    Write-Host "  IIS Application Pool optimized" -ForegroundColor Green
} else {
    Write-Host "  ⚠ IIS not installed, skipping IIS optimization" -ForegroundColor Yellow
}

# 2. Optimize Windows Services
Write-Host "`n2. Optimizing Windows Services..." -ForegroundColor Yellow

# Services to disable for better performance (non-essential)
$servicesToDisable = @(
    "DiagTrack",          # Connected User Experiences and Telemetry
    "dmwappushservice",   # Device Management Wireless Application
    "HomeGroupListener",  # HomeGroup Listener
    "HomeGroupProvider",  # HomeGroup Provider
    "WSearch",           # Windows Search (if not needed)
    "wuauserv"          # Windows Update (manage manually)
)

foreach ($service in $servicesToDisable) {
    $svc = Get-Service -Name $service -ErrorAction SilentlyContinue
    if ($svc -and $svc.Status -eq 'Running') {
        Stop-Service -Name $service -Force -ErrorAction SilentlyContinue
        Set-Service -Name $service -StartupType Disabled -ErrorAction SilentlyContinue
        Write-Host "  Disabled $service" -ForegroundColor Green
    }
}

# 3. Optimize Network Settings
Write-Host "`n3. Optimizing Network Settings..." -ForegroundColor Yellow

# Enable TCP optimizations
netsh int tcp set global autotuninglevel=normal | Out-Null
netsh int tcp set global chimney=enabled 2>$null | Out-Null
netsh int tcp set global rss=enabled | Out-Null
netsh int tcp set global netdma=enabled 2>$null | Out-Null

Write-Host "  ✓ Network settings optimized" -ForegroundColor Green

# 4. Optimize Power Settings
Write-Host "`n4. Setting High Performance Power Plan..." -ForegroundColor Yellow
powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c 2>$null
Write-Host "  ✓ High Performance power plan activated" -ForegroundColor Green

# 5. Clean Temporary Files
Write-Host "`n5. Cleaning Temporary Files..." -ForegroundColor Yellow

$tempFolders = @(
    "$env:TEMP",
    "$env:SystemRoot\Temp",
    "$env:SystemRoot\Prefetch"
)

$totalFreed = 0
foreach ($folder in $tempFolders) {
    if (Test-Path $folder) {
        $sizeBefore = (Get-ChildItem $folder -Recurse -ErrorAction SilentlyContinue | 
                      Measure-Object -Property Length -Sum).Sum / 1MB
        
        Get-ChildItem $folder -Recurse -ErrorAction SilentlyContinue | 
            Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-7) } | 
            Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
        
        $sizeAfter = (Get-ChildItem $folder -Recurse -ErrorAction SilentlyContinue | 
                     Measure-Object -Property Length -Sum).Sum / 1MB
        
        $freed = $sizeBefore - $sizeAfter
        $totalFreed += $freed
    }
}

Write-Host "  ✓ Freed $([math]::Round($totalFreed, 2)) MB of temporary files" -ForegroundColor Green

# 6. Optimize .NET Runtime
Write-Host "`n6. Optimizing .NET Runtime..." -ForegroundColor Yellow

# Set .NET runtime optimization settings
[Environment]::SetEnvironmentVariable("COMPlus_TieredCompilation", "1", "Machine")
[Environment]::SetEnvironmentVariable("COMPlus_TC_QuickJit", "1", "Machine")
[Environment]::SetEnvironmentVariable("COMPlus_TC_QuickJitForLoops", "1", "Machine")
[Environment]::SetEnvironmentVariable("COMPlus_ReadyToRun", "1", "Machine")
[Environment]::SetEnvironmentVariable("COMPlus_gcServer", "1", "Machine")
[Environment]::SetEnvironmentVariable("COMPlus_gcConcurrent", "1", "Machine")

Write-Host "  ✓ .NET Runtime optimized for performance" -ForegroundColor Green

# 7. Configure Windows Defender Exclusions
Write-Host "`n7. Configuring Windows Defender Exclusions..." -ForegroundColor Yellow

$exclusionPaths = @(
    "C:\Users\fdxadmin\source\repos\FDX.trading",
    "C:\inetpub\wwwroot",
    "C:\Program Files\dotnet",
    "C:\Users\fdxadmin\.nuget"
)

foreach ($path in $exclusionPaths) {
    if (Test-Path $path) {
        Add-MpPreference -ExclusionPath $path -ErrorAction SilentlyContinue
        Write-Host "  ✓ Added exclusion: $path" -ForegroundColor Green
    }
}

# Exclude .NET processes
$exclusionProcesses = @("dotnet.exe", "w3wp.exe", "FoodX.Admin.exe")
foreach ($process in $exclusionProcesses) {
    Add-MpPreference -ExclusionProcess $process -ErrorAction SilentlyContinue
}

# 8. Set Process Priority
Write-Host "`n8. Setting Process Priorities..." -ForegroundColor Yellow

# Set SQL Server to high priority if running
$sqlProcess = Get-Process -Name "sqlservr" -ErrorAction SilentlyContinue
if ($sqlProcess) {
    $sqlProcess.PriorityClass = [System.Diagnostics.ProcessPriorityClass]::High
    Write-Host "  ✓ SQL Server set to high priority" -ForegroundColor Green
}

# 9. Optimize Virtual Memory
Write-Host "`n9. Optimizing Virtual Memory..." -ForegroundColor Yellow

# Get system RAM
$ram = (Get-WmiObject Win32_ComputerSystem).TotalPhysicalMemory / 1GB
$minPageFile = [math]::Round($ram * 0.5, 0) * 1024
$maxPageFile = [math]::Round($ram * 1.5, 0) * 1024

# Set page file size
$pagefile = Get-WmiObject -Query "SELECT * FROM Win32_ComputerSystem" -EnableAllPrivileges
$pagefile.AutomaticManagedPagefile = $false
$pagefile.Put() | Out-Null

$pagefileSetting = Get-WmiObject -Query "SELECT * FROM Win32_PageFileSetting WHERE Name='C:\\pagefile.sys'" -EnableAllPrivileges
if ($pagefileSetting) {
    $pagefileSetting.InitialSize = $minPageFile
    $pagefileSetting.MaximumSize = $maxPageFile
    $pagefileSetting.Put() | Out-Null
    Write-Host "  ✓ Page file optimized: $minPageFile MB - $maxPageFile MB" -ForegroundColor Green
}

# 10. Display System Status
Write-Host "`n10. System Status Report:" -ForegroundColor Yellow
Write-Host "=================================" -ForegroundColor Cyan

# CPU Info
$cpu = Get-WmiObject Win32_Processor
Write-Host "CPU: $($cpu.Name)" -ForegroundColor White
Write-Host "Cores: $($cpu.NumberOfCores) | Logical Processors: $($cpu.NumberOfLogicalProcessors)" -ForegroundColor White

# Memory Info
$memory = Get-WmiObject Win32_OperatingSystem
$totalRAM = [math]::Round($memory.TotalVisibleMemorySize / 1MB, 2)
$freeRAM = [math]::Round($memory.FreePhysicalMemory / 1MB, 2)
$usedRAM = $totalRAM - $freeRAM
$percentUsed = [math]::Round(($usedRAM / $totalRAM) * 100, 2)

Write-Host "RAM: $usedRAM GB / $totalRAM GB (${percentUsed}% used)" -ForegroundColor White

# Disk Info
$disk = Get-WmiObject Win32_LogicalDisk -Filter "DeviceID='C:'"
$totalDisk = [math]::Round($disk.Size / 1GB, 2)
$freeDisk = [math]::Round($disk.FreeSpace / 1GB, 2)
$usedDisk = $totalDisk - $freeDisk
$percentDiskUsed = [math]::Round(($usedDisk / $totalDisk) * 100, 2)

Write-Host "Disk: $usedDisk GB / $totalDisk GB (${percentDiskUsed}% used)" -ForegroundColor White

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "`nSystem optimization completed successfully!" -ForegroundColor Green
Write-Host "Note: Some changes may require a restart to take full effect." -ForegroundColor Yellow

# Prompt for restart
$restart = Read-Host "`nWould you like to restart the system now? (y/n)"
if ($restart -eq 'y') {
    Write-Host "Restarting in 10 seconds..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    Restart-Computer -Force
}