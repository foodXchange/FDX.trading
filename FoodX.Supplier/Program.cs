using FoodX.Supplier.Components;
using FoodX.Supplier.Models;
using FoodX.Supplier.Services;
using FoodX.Core.Extensions;
using FoodX.Admin.Data;
using SupplierUser = FoodX.Supplier.Models.ApplicationUser;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure Key Vault (optional in Development)
try
{
    const string keyVaultName = "fdx-kv-poland";
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
    Console.WriteLine("[INFO] Azure Key Vault configured successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"[WARNING] Could not connect to Azure Key Vault: {ex.Message}");
    if (builder.Environment.IsProduction())
    {
        throw; // Re-throw in production - Key Vault is required
    }
    Console.WriteLine("[INFO] Continuing without Key Vault in Development mode");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add memory cache for performance
builder.Services.AddMemoryCache();

// Add FoodX Core services
builder.Services.AddFoodXCore(builder.Configuration);

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<FoodXDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Authentication and Identity
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddIdentityCore<SupplierUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Set to true in production
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<FoodXDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// Add HTTP client for API calls
builder.Services.AddHttpClient();

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SupplierOnly", policy => policy.RequireRole("Supplier", "SupplierAdmin", "Admin", "SuperAdmin"));
    options.AddPolicy("SupplierAdmin", policy => policy.RequireRole("SupplierAdmin", "Admin", "SuperAdmin"));
});

// Register supplier-specific services
// TODO: Add supplier services here

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add authentication endpoints
app.MapGroup("/Account").MapIdentityApi<SupplierUser>();

app.Run();
