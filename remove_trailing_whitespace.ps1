# Remove trailing whitespace from C# files
$files = Get-ChildItem -Path "FoodX.Admin" -Filter "*.cs" -Recurse
$filesFixed = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content -replace '[ \t]+(\r?\n)', '$1' -replace '[ \t]+$', ''
    
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        Write-Host "Fixed: $($file.Name)" -ForegroundColor Green
        $filesFixed++
    }
}

Write-Host "`nTotal files fixed: $filesFixed" -ForegroundColor Yellow