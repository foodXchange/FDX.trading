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
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Enable static web assets for MudBlazor and other libraries
builder.WebHost.UseStaticWebAssets();

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

// Add memory cache for performance optimization
builder.Services.AddMemoryCache();

// Configure distributed caching (Redis if available, in-memory as fallback)
var redisConnection = builder.Configuration.GetConnectionString("Redis") 
    ?? builder.Configuration["Redis:ConnectionString"];

if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "FoodXCache";
    });
    Console.WriteLine("[INFO] Redis distributed cache configured");
}
else
{
    // Use in-memory distributed cache as fallback
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("[INFO] Using in-memory distributed cache");
}

// Cache service is already registered in FoodXCore

// Add response compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] 
    {
        "application/octet-stream",
        "image/svg+xml",
        "application/font-woff2"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// Add output caching for static content
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(60)));
    options.AddPolicy("StaticAssets", builder => builder.Expire(TimeSpan.FromHours(24)));
});

// Add FoodX Core services (caching, email, etc.)
builder.Services.AddFoodXCore(builder.Configuration);

// Add Unit of Work and Repository pattern
builder.Services.AddScoped<FoodX.Admin.Repositories.IUnitOfWork, FoodX.Admin.Repositories.UnitOfWork>();

// Add services to the container with detailed error handling only in dev
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Configure Blazor Server with optimized circuit options
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(1); // Reduced from 3
        options.DisconnectedCircuitMaxRetained = 20; // Reduced from 100
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(30); // Reduced from 1 minute
        options.MaxBufferedUnacknowledgedRenderBatches = 5; // Reduced from 10 for faster response
    })
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
        options.StreamBufferCapacity = 10;
    });

// Add Vector Search Services
builder.Services.AddScoped<FoodX.Core.Services.IVectorSearchService, FoodX.Core.Services.VectorSearchService>();
builder.Services.AddScoped<FoodX.Core.Services.IEmbeddingService, FoodX.Core.Services.AzureOpenAIEmbeddingService>();
builder.Services.AddScoped<FoodX.Admin.Services.TestUserService>();

// Add AI Request Analyzer Service
builder.Services.AddHttpClient();
builder.Services.AddScoped<FoodX.Admin.Services.IAIRequestAnalyzer, FoodX.Admin.Services.AIRequestAnalyzer>();
builder.Services.AddScoped<FoodX.Admin.Services.SupplierSearchService>();

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
    ? $"{connectionString[..passwordIndex]}Password=***"
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

// Add DbContextFactory for Blazor Server components to avoid disposed context issues
builder.Services.AddDbContextFactory<ApplicationDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<FoodX.Admin.Data.PerformanceInterceptor>();
    FoodX.Admin.Data.DatabaseConfiguration.ConfigureDbContext(options, optimizedConnectionString, false);
    options.AddInterceptors(interceptor);
}, ServiceLifetime.Scoped);

builder.Services.AddDbContextFactory<FoodXDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<FoodX.Admin.Data.PerformanceInterceptor>();
    FoodX.Admin.Data.DatabaseConfiguration.ConfigureDbContext(options, optimizedConnectionString, false);
    options.AddInterceptors(interceptor);
}, ServiceLifetime.Scoped);

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

// Register Portal Context Service for SuperAdmin portal switching
builder.Services.AddScoped<FoodX.Admin.Services.IPortalContextService, FoodX.Admin.Services.PortalContextService>();

// Add Dashboard Data Service with caching
builder.Services.AddScoped<FoodX.Admin.Services.IDashboardDataService, FoodX.Admin.Services.DashboardDataService>();

// Configure Azure OpenAI with credentials from Azure Key Vault
var azureOpenAIKey = builder.Configuration["AzureOpenAI-ApiKey"] 
    ?? builder.Configuration["AzureOpenAI:ApiKey"];
var azureOpenAIEndpoint = builder.Configuration["AzureOpenAI-Endpoint"] 
    ?? builder.Configuration["AzureOpenAI:Endpoint"];

if (!string.IsNullOrEmpty(azureOpenAIKey))
{
    // Store in configuration for services to access
    builder.Configuration["AzureOpenAI:ApiKey"] = azureOpenAIKey;
    builder.Configuration["AzureOpenAI:Endpoint"] = azureOpenAIEndpoint ?? "https://polandcentral.api.cognitive.microsoft.com/";
    Console.WriteLine($"[INFO] Azure OpenAI configured from Key Vault (endpoint: {azureOpenAIEndpoint ?? "default"})");
}
else
{
    Console.WriteLine("[WARNING] Azure OpenAI API key not found in Azure Key Vault");
}

// Configure SendGrid with API key from Azure Key Vault
var sendGridApiKey = builder.Configuration["SendGridApiKey"];
if (!string.IsNullOrEmpty(sendGridApiKey))
{
    builder.Services.AddSingleton<SendGrid.ISendGridClient>(new SendGrid.SendGridClient(sendGridApiKey));
    Console.WriteLine($"[INFO] SendGrid configured with API key from Azure Key Vault (key starts with: {sendGridApiKey[..Math.Min(10, sendGridApiKey.Length)]})");
}
else
{
    Console.WriteLine("[WARNING] SendGrid API key not found in Azure Key Vault");
}

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

// Enable response compression (must be before other middleware)
app.UseResponseCompression();

// Enable output caching
app.UseOutputCache();

// Configure static file options with caching
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 year (versioned files)
        const int durationInSeconds = 365 * 24 * 60 * 60;
        ctx.Context.Response.Headers.Append(
            "Cache-Control", $"public, max-age={durationInSeconds}");
        
        // Add ETag for cache validation
        ctx.Context.Response.Headers.Append("ETag", 
            $"\"{ctx.File.LastModified.ToFileTime()}\"");
    }
});

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