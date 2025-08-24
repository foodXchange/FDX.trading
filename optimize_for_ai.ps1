# Optimize Azure Credits for AI Services
# This script helps you save VM costs to use credits for AI

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "OPTIMIZE AZURE CREDITS FOR AI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$ResourceGroup = "FDX-DOTNET-RG"
$VMName = "fdx-win-desktop"

# Check current VM size and estimate savings
$currentSize = az vm show --resource-group $ResourceGroup --name $VMName --query "hardwareProfile.vmSize" -o tsv

$monthlyCosts = @{
    "Standard_B2ms" = 90
    "Standard_B4ms" = 181
    "Standard_D4s_v5" = 220
    "Standard_D8s_v5" = 440
}

$aiCreditsAvailable = @{
    "Standard_B2ms" = 91  # Saves $91/month vs B4ms
    "Standard_B4ms" = 0   # Current baseline
    "Standard_D4s_v5" = -39  # Costs $39 more
}

Write-Host "`nCurrent VM: $currentSize" -ForegroundColor Yellow
Write-Host "Monthly Cost: `$$($monthlyCosts[$currentSize])" -ForegroundColor Yellow

if ($currentSize -eq "Standard_B4ms") {
    Write-Host "`n💡 RECOMMENDATION:" -ForegroundColor Green
    Write-Host "Downsize to B2ms to save `$91/month for AI services!" -ForegroundColor Green
    
    Write-Host "`nWith `$91/month you could use:" -ForegroundColor Cyan
    Write-Host "  • 500,000 GPT-4 tokens" -ForegroundColor White
    Write-Host "  • 5,000,000 GPT-3.5 tokens" -ForegroundColor White
    Write-Host "  • 1,000 hours of Cognitive Services" -ForegroundColor White
    Write-Host "  • 100 hours of GPU compute for ML" -ForegroundColor White
    
    $answer = Read-Host "`nDownsize to B2ms now? (y/n)"
    if ($answer -eq "y") {
        Write-Host "Resizing VM to B2ms..." -ForegroundColor Yellow
        az vm resize --resource-group $ResourceGroup --name $VMName --size Standard_B2ms
        Write-Host "✓ VM downsized! You now have `$91/month more for AI!" -ForegroundColor Green
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "AI SERVICES YOU CAN USE WITH CREDITS:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n1. Azure OpenAI Service" -ForegroundColor Yellow
Write-Host "   - GPT-4, GPT-3.5, DALL-E, Embeddings" -ForegroundColor Gray
Write-Host "   - Cost: `$0.03-0.06 per 1K tokens" -ForegroundColor Gray

Write-Host "`n2. Azure Cognitive Services" -ForegroundColor Yellow
Write-Host "   - Computer Vision, Speech, Translation" -ForegroundColor Gray
Write-Host "   - Cost: `$1-2 per 1K transactions" -ForegroundColor Gray

Write-Host "`n3. Azure Machine Learning" -ForegroundColor Yellow
Write-Host "   - Training compute, AutoML, Endpoints" -ForegroundColor Gray
Write-Host "   - Cost: `$0.90/hour for CPU, `$2.50/hour for GPU" -ForegroundColor Gray

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "QUICK VM MANAGEMENT COMMANDS:" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "Downsize (save money): az vm resize --resource-group $ResourceGroup --name $VMName --size Standard_B2ms" -ForegroundColor Gray
Write-Host "Normal size: az vm resize --resource-group $ResourceGroup --name $VMName --size Standard_B4ms" -ForegroundColor Gray
Write-Host "Stop VM (save 100%): az vm deallocate --resource-group $ResourceGroup --name $VMName" -ForegroundColor Gray
Write-Host "Start VM: az vm start --resource-group $ResourceGroup --name $VMName" -ForegroundColor Gray