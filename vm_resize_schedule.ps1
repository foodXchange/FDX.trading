# Azure VM Resize Schedule Script
# This script can be used with Azure Automation or Task Scheduler

param(
    [Parameter(Mandatory=$false)]
    [string]$Action = "status", # status, upsize, downsize
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "fdx-dotnet-rg",
    
    [Parameter(Mandatory=$false)]
    [string]$VMName = "fdx-win-desktop"
)

# VM Size configurations
$WorkHoursSize = "Standard_B4ms"    # 4 vCPUs, 16 GB RAM - Current size
$HighPerfSize = "Standard_B8ms"     # 8 vCPUs, 32 GB RAM - For heavy workloads
$OffHoursSize = "Standard_B2ms"     # 2 vCPUs, 8 GB RAM - For cost savings

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AZURE VM RESIZE SCHEDULER" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Check if Azure CLI is logged in
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Please login to Azure first: az login" -ForegroundColor Red
    exit 1
}

# Get current VM status
Write-Host "`nGetting VM information..." -ForegroundColor Yellow
$vm = az vm show --resource-group $ResourceGroup --name $VMName 2>$null | ConvertFrom-Json

if (-not $vm) {
    Write-Host "VM not found: $VMName in resource group $ResourceGroup" -ForegroundColor Red
    exit 1
}

$currentSize = $vm.hardwareProfile.vmSize
$vmStatus = az vm get-instance-view --resource-group $ResourceGroup --name $VMName --query "instanceView.statuses[?starts_with(code, 'PowerState')].displayStatus" -o tsv

Write-Host "Current VM Size: $currentSize" -ForegroundColor Cyan
Write-Host "Current Status: $vmStatus" -ForegroundColor Cyan

function Resize-VM {
    param(
        [string]$NewSize
    )
    
    if ($currentSize -eq $NewSize) {
        Write-Host "VM is already sized as $NewSize" -ForegroundColor Yellow
        return
    }
    
    Write-Host "`nResizing VM from $currentSize to $NewSize..." -ForegroundColor Yellow
    
    # Check if VM needs to be stopped
    if ($vmStatus -eq "VM running") {
        Write-Host "Stopping VM..." -ForegroundColor Yellow
        az vm stop --resource-group $ResourceGroup --name $VMName
    }
    
    # Resize the VM
    Write-Host "Resizing VM..." -ForegroundColor Yellow
    az vm resize --resource-group $ResourceGroup --name $VMName --size $NewSize
    
    # Start the VM
    Write-Host "Starting VM..." -ForegroundColor Yellow
    az vm start --resource-group $ResourceGroup --name $VMName
    
    Write-Host "VM successfully resized to $NewSize" -ForegroundColor Green
}

# Perform action based on parameter
switch ($Action.ToLower()) {
    "status" {
        Write-Host "`nVM Resize Options:" -ForegroundColor Yellow
        Write-Host "  Off-Hours Size: $OffHoursSize (2 vCPUs, 8 GB RAM)" -ForegroundColor Gray
        Write-Host "  Work Hours Size: $WorkHoursSize (4 vCPUs, 16 GB RAM)" -ForegroundColor Gray
        Write-Host "  High Performance: $HighPerfSize (8 vCPUs, 32 GB RAM)" -ForegroundColor Gray
        
        # Get current time
        $currentHour = (Get-Date).Hour
        $dayOfWeek = (Get-Date).DayOfWeek
        
        Write-Host "`nSchedule Recommendation:" -ForegroundColor Yellow
        if ($dayOfWeek -eq "Saturday" -or $dayOfWeek -eq "Sunday") {
            Write-Host "  Weekend: Consider using $OffHoursSize for cost savings" -ForegroundColor Green
        } elseif ($currentHour -ge 9 -and $currentHour -lt 18) {
            Write-Host "  Work Hours (9 AM - 6 PM): Use $WorkHoursSize or $HighPerfSize" -ForegroundColor Green
        } else {
            Write-Host "  Off Hours: Consider using $OffHoursSize for cost savings" -ForegroundColor Green
        }
    }
    
    "downsize" {
        Write-Host "`nDownsizing VM for off-hours..." -ForegroundColor Yellow
        Resize-VM -NewSize $OffHoursSize
    }
    
    "upsize" {
        Write-Host "`nUpsizing VM for work hours..." -ForegroundColor Yellow
        Resize-VM -NewSize $WorkHoursSize
    }
    
    "highperf" {
        Write-Host "`nUpsizing VM for high performance..." -ForegroundColor Yellow
        Resize-VM -NewSize $HighPerfSize
    }
    
    default {
        Write-Host "Invalid action. Use: status, upsize, downsize, or highperf" -ForegroundColor Red
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "OPERATION COMPLETE" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan