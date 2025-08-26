# PowerShell script to fix remaining portal page issues

Write-Host "Fixing remaining build errors in portal pages..." -ForegroundColor Green

# Fix MudList and MudListItem in ImportProducts
$file = "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin\Components\Pages\Portal\Supplier\ImportProducts.razor"
$content = Get-Content $file -Raw
$content = $content -replace '<MudList>', '<MudList T="string">'
$content = $content -replace '<MudListItem Icon', '<MudListItem T="string" Icon'
Set-Content $file $content

# Fix MudList and MudListItem in RFQs
$file = "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin\Components\Pages\Portal\Supplier\RFQs.razor"
$content = Get-Content $file -Raw
$content = $content -replace '<MudList>', '<MudList T="string">'
$content = $content -replace '<MudListItem Icon', '<MudListItem T="string" Icon'
Set-Content $file $content

# Fix MudList and MudListItem in Profile
$file = "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin\Components\Pages\Portal\Supplier\Profile.razor"
$content = Get-Content $file -Raw
$content = $content -replace '<MudList>', '<MudList T="string">'
$content = $content -replace '<MudListItem Icon', '<MudListItem T="string" Icon'
$content = $content -replace '<ButtonTemplate>', '<ActivatorContent>'
$content = $content -replace '</ButtonTemplate>', '</ActivatorContent>'
Set-Content $file $content

# Add NavigationManager and ISnackbar injections to all supplier pages
$supplierPages = @(
    "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin\Components\Pages\Portal\Supplier\RFQs.razor",
    "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin\Components\Pages\Portal\Supplier\Quotes.razor",
    "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin\Components\Pages\Portal\Supplier\Orders.razor",
    "C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin\Components\Pages\Portal\Supplier\ImportProducts.razor"
)

foreach ($file in $supplierPages) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw
        
        # Check if NavigationManager is already injected
        if ($content -notmatch '@inject NavigationManager') {
            # Add injections after @attribute line
            $content = $content -replace '(@attribute \[Authorize[^\]]+\])', '$1`r`n@inject NavigationManager Navigation`r`n@inject ISnackbar Snackbar'
        }
        
        # Fix Snackbar.Add calls (change from 2 arguments to 1)
        $content = $content -replace 'Snackbar\.Add\([^,]+,\s*Severity\.[^)]+\)', 'Snackbar.Add($1)'
        
        Set-Content $file $content
    }
}

Write-Host "Fixes applied. Running build to check results..." -ForegroundColor Yellow