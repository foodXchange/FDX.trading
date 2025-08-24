# Upgrade to High-Performance VM
# Get REAL performance, not burstable throttled CPU!

param(
    [string]$Action = "check"
)

$ResourceGroup = "FDX-DOTNET-RG"
$VMName = "fdx-win-desktop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VM PERFORMANCE UPGRADE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$currentSize = az vm show --resource-group $ResourceGroup --name $VMName --query "hardwareProfile.vmSize" -o tsv

Write-Host "`nCurrent VM: $currentSize" -ForegroundColor Yellow

if ($currentSize -like "*B*") {
    Write-Host "⚠️  You're on BURSTABLE series - CPU is THROTTLED!" -ForegroundColor Red
    Write-Host "   Only 20-40% baseline CPU, rest needs credits" -ForegroundColor Red
}

Write-Host "`n🚀 HIGH-PERFORMANCE OPTIONS:" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n1. Standard_D4s_v5 ⭐ RECOMMENDED" -ForegroundColor Green
Write-Host "   • 4 vCPUs (100% always) Intel Xeon" -ForegroundColor White
Write-Host "   • 16 GB RAM, 150 GB Temp SSD" -ForegroundColor White
Write-Host "   • ~$220/month (+$39 vs B4ms)" -ForegroundColor Yellow
Write-Host "   • 2-3x faster than B4ms" -ForegroundColor Green

Write-Host "`n2. Standard_D4as_v5 💰 BEST PRICE/PERF" -ForegroundColor Cyan
Write-Host "   • 4 vCPUs (100% always) AMD EPYC" -ForegroundColor White
Write-Host "   • 16 GB RAM, 150 GB Temp SSD" -ForegroundColor White
Write-Host "   • ~$195/month (+$14 vs B4ms)" -ForegroundColor Yellow
Write-Host "   • 2x faster, AMD processor" -ForegroundColor Green

Write-Host "`n3. Standard_D8s_v5 💪 MAXIMUM POWER" -ForegroundColor Yellow
Write-Host "   • 8 vCPUs (100% always) Intel Xeon" -ForegroundColor White
Write-Host "   • 32 GB RAM, 300 GB Temp SSD" -ForegroundColor White
Write-Host "   • ~$440/month (+$259 vs B4ms)" -ForegroundColor Yellow
Write-Host "   • 4-5x faster, run everything!" -ForegroundColor Green

Write-Host "`n4. Standard_D4ds_v5 WITH PREMIUM SSD" -ForegroundColor Magenta
Write-Host "   - Same as D4s_v5 + Premium SSD support" -ForegroundColor White
Write-Host "   - ~`$240/month (+`$59 vs B4ms)" -ForegroundColor Yellow
Write-Host "   - Best for database/IO heavy work" -ForegroundColor Green

if ($Action -eq "check") {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "To upgrade, run:" -ForegroundColor Yellow
    Write-Host ".\upgrade_vm_performance.ps1 -Action upgrade" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor Cyan
    exit
}

if ($Action -eq "upgrade") {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "SELECT YOUR UPGRADE:" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Cyan
    
    Write-Host "1. D4s_v5 - Balanced Intel (Recommended)" -ForegroundColor Green
    Write-Host "2. D4as_v5 - Balanced AMD (Cheapest)" -ForegroundColor Cyan
    Write-Host "3. D8s_v5 - Maximum Performance" -ForegroundColor Yellow
    Write-Host "4. D4ds_v5 - With Premium SSD" -ForegroundColor Magenta
    Write-Host "5. Cancel" -ForegroundColor Red
    
    $choice = Read-Host "`nEnter choice (1-5)"
    
    $newSize = switch ($choice) {
        "1" { "Standard_D4s_v5" }
        "2" { "Standard_D4as_v5" }
        "3" { "Standard_D8s_v5" }
        "4" { "Standard_D4ds_v5" }
        default { $null }
    }
    
    if ($newSize) {
        Write-Host "`nUPGRADING TO $newSize" -ForegroundColor Green
        Write-Host "This will restart your VM (3-5 minutes)" -ForegroundColor Yellow
        
        $confirm = Read-Host "Proceed? (y/n)"
        if ($confirm -eq "y") {
            Write-Host "Starting upgrade..." -ForegroundColor Yellow
            
            # Perform the resize
            az vm resize --resource-group $ResourceGroup --name $VMName --size $newSize
            
            if ($?) {
                Write-Host "`nSUCCESS! VM upgraded to $newSize" -ForegroundColor Green
                Write-Host "Your VM now has:" -ForegroundColor Cyan
                
                switch ($newSize) {
                    "Standard_D4s_v5" {
                        Write-Host "• 100% CPU availability (no throttling!)" -ForegroundColor Green
                        Write-Host "• 3x faster compile times" -ForegroundColor Green
                        Write-Host "• Better Visual Studio performance" -ForegroundColor Green
                    }
                    "Standard_D4as_v5" {
                        Write-Host "• AMD EPYC processor power" -ForegroundColor Green
                        Write-Host "• Great price/performance ratio" -ForegroundColor Green
                        Write-Host "• 100% CPU always available" -ForegroundColor Green
                    }
                    "Standard_D8s_v5" {
                        Write-Host "• 8 powerful vCPUs" -ForegroundColor Green
                        Write-Host "• 32 GB RAM for heavy workloads" -ForegroundColor Green
                        Write-Host "• Run multiple apps smoothly" -ForegroundColor Green
                    }
                }
                
                # Calculate monthly savings potential
                Write-Host "`nTIP: To optimize costs:" -ForegroundColor Yellow
                Write-Host "• Stop VM at night: az vm deallocate --resource-group $ResourceGroup --name $VMName" -ForegroundColor Gray
                Write-Host "• This saves 50% if you work 12 hours/day" -ForegroundColor Gray
            }
        }
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Need help? Options:" -ForegroundColor Yellow
Write-Host "• Check current: .\upgrade_vm_performance.ps1" -ForegroundColor White
Write-Host "• Upgrade now: .\upgrade_vm_performance.ps1 -Action upgrade" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan