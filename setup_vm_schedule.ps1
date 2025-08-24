# Setup VM Auto-Resize Schedule with Windows Task Scheduler
# Run this script as Administrator

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SETTING UP VM AUTO-RESIZE SCHEDULE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$scriptPath = "C:\Users\fdxadmin\source\repos\FDX.trading\vm_resize_schedule.ps1"

# Create scheduled task for downsizing at 11 PM
$downsizeAction = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-ExecutionPolicy Bypass -File `"$scriptPath`" -Action downsize"

$downsizeTrigger = New-ScheduledTaskTrigger -Daily -At "11:00PM"

$downsizeSettings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RunOnlyIfNetworkAvailable

Register-ScheduledTask `
    -TaskName "VM Auto Downsize" `
    -Description "Automatically downsize VM to B2ms at 11 PM for cost savings" `
    -Action $downsizeAction `
    -Trigger $downsizeTrigger `
    -Settings $downsizeSettings `
    -User "SYSTEM" `
    -RunLevel Highest `
    -Force

Write-Host "✓ Downsize task created (11 PM daily)" -ForegroundColor Green

# Create scheduled task for upsizing at 8 AM
$upsizeAction = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-ExecutionPolicy Bypass -File `"$scriptPath`" -Action upsize"

$upsizeTrigger = New-ScheduledTaskTrigger -Daily -At "8:00AM"

$upsizeSettings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RunOnlyIfNetworkAvailable

Register-ScheduledTask `
    -TaskName "VM Auto Upsize" `
    -Description "Automatically upsize VM to B4ms at 8 AM for work hours" `
    -Action $upsizeAction `
    -Trigger $upsizeTrigger `
    -Settings $upsizeSettings `
    -User "SYSTEM" `
    -RunLevel Highest `
    -Force

Write-Host "✓ Upsize task created (8 AM daily)" -ForegroundColor Green

# Create weekend downsize task
$weekendAction = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-ExecutionPolicy Bypass -File `"$scriptPath`" -Action downsize"

$weekendTrigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Saturday -At "12:01AM"

Register-ScheduledTask `
    -TaskName "VM Weekend Downsize" `
    -Description "Keep VM downsized during weekends" `
    -Action $weekendAction `
    -Trigger $weekendTrigger `
    -Settings $downsizeSettings `
    -User "SYSTEM" `
    -RunLevel Highest `
    -Force

Write-Host "✓ Weekend downsize task created" -ForegroundColor Green

Write-Host "`nSchedule Summary:" -ForegroundColor Yellow
Write-Host "  Mon-Fri 8 AM: Upsize to B4ms (4 vCPUs, 16 GB)" -ForegroundColor Gray
Write-Host "  Mon-Fri 11 PM: Downsize to B2ms (2 vCPUs, 8 GB)" -ForegroundColor Gray
Write-Host "  Weekends: Stay at B2ms for cost savings" -ForegroundColor Gray

Write-Host "`nEstimated Monthly Savings:" -ForegroundColor Yellow
Write-Host "  Without schedule: $133/month (B4ms 24/7)" -ForegroundColor Gray
Write-Host "  With schedule: ~$87/month (40% savings)" -ForegroundColor Green

Write-Host "`nTo view scheduled tasks, run:" -ForegroundColor Cyan
Write-Host "  Get-ScheduledTask | Where-Object {`$_.TaskName -like 'VM*'}" -ForegroundColor Gray

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SCHEDULE SETUP COMPLETE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan