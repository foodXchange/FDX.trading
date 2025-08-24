using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using FoodX.Admin.Components;
using FoodX.Admin.Components.Account;
using FoodX.Admin.Data;
using FoodX.Core.Extensions;
using System.Globalization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure Key Vault (only in Production)
if (builder.Environment.IsProduction())
{
    var keyVaultName = "fdx-kv-poland";
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]
        ?? builder.Configuration["ApplicationInsights--ConnectionString"];
});

// Configure Globalization (Israel/Jerusalem timezone)
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

// Add MudBlazor services
builder.Services.AddMudServices();

// Add FoodX Core services (caching, email, etc.)
builder.Services.AddFoodXCore(builder.Configuration);

// Add Unit of Work and Repository pattern
builder.Services.AddScoped<FoodX.Admin.Repositories.IUnitOfWork, FoodX.Admin.Repositories.UnitOfWork>();

// Add services to the container with detailed error handling only in dev
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Configure Blazor Server with circuit options
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.DisconnectedCircuitMaxRetained = 100;
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
        options.MaxBufferedUnacknowledgedRenderBatches = 10;
    });

// Add Vector Search Services
builder.Services.AddScoped<FoodX.Core.Services.IVectorSearchService, FoodX.Core.Services.VectorSearchService>();
builder.Services.AddHttpClient<FoodX.Core.Services.IEmbeddingService, FoodX.Core.Services.AzureOpenAIEmbeddingService>();
builder.Services.AddScoped<FoodX.Admin.Services.TestUserService>();

// Add Authentication
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

// Configure cookie policy for security
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // In development, make authentication optional
    if (builder.Environment.IsDevelopment())
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 200;
            context.Response.Redirect("/");
            return Task.CompletedTask;
        };
    }
});

// Get connection string from configuration (Key Vault in Production, appsettings in Development)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DefaultConnection"]
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

// Log the connection string for debugging (remove sensitive parts)
var passwordIndex = connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
var debugConnStr = passwordIndex >= 0
    ? string.Concat(connectionString.AsSpan(0, passwordIndex), "Password=***")
    : connectionString;
Console.WriteLine($"[DEBUG] Using connection string: {debugConnStr}");

// Optimize connection string
var optimizedConnectionString = FoodX.Admin.Data.DatabaseConfiguration.OptimizeConnectionString(connectionString);

// Create performance interceptor
builder.Services.AddSingleton<FoodX.Admin.Data.PerformanceInterceptor>();

// Add both contexts with resilient SQL configuration
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<FoodX.Admin.Data.PerformanceInterceptor>();
    FoodX.Admin.Data.DatabaseConfiguration.ConfigureDbContext(options, optimizedConnectionString, false); // Disable sensitive logging in production
    options.AddInterceptors(interceptor);
});

builder.Services.AddDbContext<FoodXDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<FoodX.Admin.Data.PerformanceInterceptor>();
    FoodX.Admin.Data.DatabaseConfiguration.ConfigureDbContext(options, optimizedConnectionString, false); // Disable sensitive logging in production
    options.AddInterceptors(interceptor);
});

// Add database developer page exception filter only in development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

// Configure Identity with proper settings
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false; // Set to true in production
        options.SignIn.RequireConfirmedEmail = false; // Set to true in production
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Register email sender (replace with real implementation in production)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Register role navigation service
builder.Services.AddScoped<FoodX.Admin.Services.IRoleNavigationService, FoodX.Admin.Services.RoleNavigationService>();

// Register Magic Link and Email services
builder.Services.AddScoped<FoodX.Admin.Services.IMagicLinkService, FoodX.Admin.Services.MagicLinkService>();
// Register dual-mode email service (supports both API and SMTP)
builder.Services.AddScoped<FoodX.Admin.Services.ISendGridEmailService, FoodX.Admin.Services.DualModeEmailService>();
// Keep old IEmailService for backward compatibility
builder.Services.AddScoped<FoodX.Admin.Services.IEmailService, FoodX.Admin.Services.EmailService>();

// Register test user service for development
builder.Services.AddScoped<FoodX.Admin.Services.TestUserService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("identity-db")
    .AddDbContextCheck<FoodXDbContext>("foodx-db")
    .AddSqlServer(connectionString, name: "sql-server");

// Add logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add EventLog only on Windows in production
if (!builder.Environment.IsDevelopment() && OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog();
}

// Add Authorization policies using AddAuthorizationBuilder
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
    .AddPolicy("RequireBuyerRole", policy => policy.RequireRole("Buyer", "Admin"))
    .AddPolicy("RequireSellerRole", policy => policy.RequireRole("Seller", "Admin"))
    .AddPolicy("RequireAgentRole", policy => policy.RequireRole("Agent", "Admin"));

// Add CORS if needed for API access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://localhost:5001", "https://foodx.trading")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Use CORS if configured
app.UseCors("AllowSpecificOrigins");

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Map API controllers
app.MapControllers();

// Map health checks endpoint
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Temporary endpoint to reset test passwords (REMOVE IN PRODUCTION)
if (app.Environment.IsDevelopment())
{
    app.MapGet("/reset-test-passwords", async (UserManager<ApplicationUser> userManager) =>
    {
        var testUsers = new Dictionary<string, string>
        {
            { "admin1@test.com", "Admin1@Pass123" },
            { "buyer1@test.com", "Buyer1@Pass123" },
            { "supplier1@test.com", "Supplier1@Pass123" }
        };

        var results = new List<string>();

        foreach (var kvp in testUsers)
        {
            var user = await userManager.FindByEmailAsync(kvp.Key);
            if (user != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, kvp.Value);

                if (result.Succeeded)
                {
                    results.Add($"[OK] Reset password for: {kvp.Key}");
                }
                else
                {
                    results.Add($"[FAILED] Could not reset password for: {kvp.Key} - {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                results.Add($"[NOT FOUND] User: {kvp.Key}");
            }
        }

        return string.Join("\n", results);
    });
}

// Seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
    if (roleManager != null)
    {
        string[] roles = { "SuperAdmin", "Admin", "Buyer", "Supplier", "Agent", "Expert" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}

await app.RunAsync();