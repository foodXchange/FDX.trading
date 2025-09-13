using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using FoodX.Admin.Components;
using FoodX.Admin.Components.Account;
using FoodX.Admin.Data;
using FoodX.Admin.Services;
using FoodX.Core.Extensions;
using System.Globalization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Azure.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Hangfire;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Enable static web assets for MudBlazor and other libraries
builder.WebHost.UseStaticWebAssets();

// Configure Azure Key Vault (only when enabled)
var useKeyVault = builder.Configuration.GetValue<bool>("AzureKeyVault:UseKeyVault", true);
if (useKeyVault)
{
    try
    {
        const string keyVaultName = "fdx-kv-poland";
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        var tenantId = builder.Configuration["AzureKeyVault:TenantId"];

        // Use tenant-specific credential if provided
        var credential = !string.IsNullOrEmpty(tenantId)
            ? new DefaultAzureCredential(new DefaultAzureCredentialOptions { TenantId = tenantId })
            : new DefaultAzureCredential();

        builder.Configuration.AddAzureKeyVault(keyVaultUri, credential);
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
}
else
{
    Console.WriteLine("[INFO] Azure Key Vault disabled - using direct configuration");
}

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]
        ?? builder.Configuration["ApplicationInsights--ConnectionString"];
});

// Add custom metrics service
builder.Services.AddSingleton<FoodX.Admin.Services.IMetricsService, FoodX.Admin.Services.MetricsService>();

// Add API Key service
builder.Services.AddScoped<FoodX.Admin.Services.IApiKeyService, FoodX.Admin.Services.ApiKeyService>();

// Configure Globalization (Israel/Jerusalem timezone)
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

// Add MudBlazor services
builder.Services.AddMudServices();

// Add memory cache with optimized settings
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024 * 1024 * 100; // 100MB
    options.CompactionPercentage = 0.25;
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

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

// Use optimized compression configuration
builder.Services.AddOptimizedCompression();

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// Use optimized caching configuration
builder.Services.AddOptimizedCaching();

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Specific rate limit for authentication endpoints
    options.AddPolicy("Authentication", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    // API rate limit
    options.AddPolicy("Api", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 30,
                QueueLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4
            }));

    // Strict rate limit for sensitive operations
    options.AddPolicy("Strict", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new TokenBucketRateLimiterOptions
            {
                AutoReplenishment = true,
                TokenLimit = 10,
                QueueLimit = 0,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 10
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsync(
                $"Too many requests. Please retry after {retryAfter.TotalSeconds} second(s).", 
                cancellationToken);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync(
                "Too many requests. Please retry later.", 
                cancellationToken);
        }
    };
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

// Use optimized Hangfire configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       builder.Configuration.GetConnectionString("FdxDb") ??
                       throw new InvalidOperationException("Connection string not found");

builder.Services.AddOptimizedHangfire(connectionString);

// Register services
builder.Services.AddScoped<IOrderWorkflowService, OrderWorkflowService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Register background jobs
builder.Services.AddScoped<FoodX.Admin.Services.BackgroundJobs.IInvoiceGenerationJob, FoodX.Admin.Services.BackgroundJobs.InvoiceGenerationJob>();
builder.Services.AddScoped<FoodX.Admin.Services.BackgroundJobs.IWorkflowAutomationJob, FoodX.Admin.Services.BackgroundJobs.WorkflowAutomationJob>();

// Configure Blazor Server with optimized circuit options
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(30); // Further optimized
        options.DisconnectedCircuitMaxRetained = 10; // Further reduced
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(10); // Further optimized
        options.MaxBufferedUnacknowledgedRenderBatches = 2; // Optimized for performance
    })
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
        options.StreamBufferCapacity = 10;
        options.MaximumParallelInvocationsPerClient = 2; // Added optimization
    });

// Add Vector Search Services
builder.Services.AddScoped<FoodX.Core.Services.IVectorSearchService, FoodX.Core.Services.VectorSearchService>();
builder.Services.AddScoped<FoodX.Core.Services.IEmbeddingService, FoodX.Core.Services.AzureOpenAIEmbeddingService>();
builder.Services.AddScoped<FoodX.Admin.Services.TestUserService>();

// Add AI Request Analyzer Service
builder.Services.AddHttpClient();

// Add RFQ and Product Sourcing Services
builder.Services.AddScoped<FoodX.Admin.Services.IRFQManagementService, FoodX.Admin.Services.RFQManagementService>();
builder.Services.AddScoped<FoodX.Admin.Services.ISupplierMatchingService, FoodX.Admin.Services.SupplierMatchingService>();
builder.Services.AddScoped<FoodX.Admin.Services.IEmailCampaignService, FoodX.Admin.Services.EmailCampaignService>();
// builder.Services.AddScoped<IProductCatalogImportService, ProductCatalogImportService>(); // Removed service
builder.Services.AddScoped<FoodX.Admin.Services.IAIRequestAnalyzer, FoodX.Admin.Services.AIRequestAnalyzer>();
builder.Services.AddScoped<FoodX.Admin.Services.SupplierSearchService>();

// Add Billing System Services
builder.Services.AddScoped<FoodX.Admin.Services.ICommissionCalculator, FoodX.Admin.Services.CommissionCalculator>();
// builder.Services.AddScoped<FoodX.Admin.Services.IInvoiceService, FoodX.Admin.Services.InvoiceService>();
builder.Services.AddScoped<FoodX.Admin.Services.IOrderService, FoodX.Admin.Services.OrderService>();
// builder.Services.AddScoped<FoodX.Admin.Services.IShipmentService, FoodX.Admin.Services.ShipmentService>();
// builder.Services.AddScoped<FoodX.Admin.Services.IPaymentProcessor, FoodX.Admin.Services.PaymentProcessor>();
builder.Services.AddScoped<FoodX.Admin.Services.RecurringOrderService>();
builder.Services.AddScoped<FoodX.Admin.Services.IPerformanceAnalyticsService, FoodX.Admin.Services.PerformanceAnalyticsService>();

// Add Workflow and Domain Event Services
builder.Services.AddSingleton<FoodX.Admin.Services.DomainEvents.IDomainEventService, FoodX.Admin.Services.DomainEvents.DomainEventService>();
// builder.Services.AddScoped<FoodX.Admin.Services.IOrderWorkflowService, FoodX.Admin.Services.OrderWorkflowService>();

// Add other caching services
builder.Services.AddScoped<FoodX.Admin.Services.ICacheService, FoodX.Admin.Services.CacheService>();
builder.Services.AddScoped<FoodX.Admin.Services.ICacheInvalidationService, FoodX.Admin.Services.CacheInvalidationService>();
builder.Services.AddScoped<FoodX.Admin.Services.ICachedProductService, FoodX.Admin.Services.CachedProductService>();

// Add Database Consolidation Services
builder.Services.AddScoped<FoodX.Admin.Services.IDatabaseConsolidationService, FoodX.Admin.Services.DatabaseConsolidationService>();
builder.Services.AddScoped<FoodX.Admin.Services.IEntityBridgeService, FoodX.Admin.Services.EntityBridgeService>();

// Add Email Service Client
builder.Services.AddHttpClient<FoodX.Admin.Services.IEmailServiceClient, FoodX.Admin.Services.EmailServiceClient>(client =>
{
    var emailServiceUrl = builder.Configuration["EmailService:BaseUrl"] ?? "https://localhost:7001";
    client.BaseAddress = new Uri(emailServiceUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Use optimized SignalR configuration
builder.Services.AddOptimizedSignalR(redisConnection);
// builder.Services.AddScoped<FoodX.Admin.Services.INotificationService, FoodX.Admin.Services.NotificationService>();
// builder.Services.AddSingleton<FoodX.Admin.Services.IUserConnectionManager, FoodX.Admin.Services.UserConnectionManager>();
// Navigation and search services
builder.Services.AddScoped<FoodX.Admin.Services.IRoleNavigationService, FoodX.Admin.Services.RoleNavigationService>();
// builder.Services.AddScoped<FoodX.Admin.Services.IGlobalSearchService, FoodX.Admin.Services.GlobalSearchService>();

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
    .AddScheme<FoodX.Admin.Services.ApiKeyAuthenticationOptions, FoodX.Admin.Services.ApiKeyAuthenticationHandler>(
        FoodX.Admin.Services.ApiKeyAuthenticationOptions.DefaultScheme, 
        options => 
        {
            options.HeaderName = "X-Api-Key";
            options.QueryParameterName = "api_key";
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

// Connection string was already retrieved earlier for Hangfire
// Get connection string from configuration (Key Vault in Production, appsettings in Development)
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DefaultConnection"]
    ?? connectionString;

// Log the connection string for debugging (remove sensitive parts)
var passwordIndex = dbConnectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
var debugConnStr = passwordIndex >= 0
    ? $"{dbConnectionString[..passwordIndex]}Password=***"
    : dbConnectionString;
Console.WriteLine($"[DEBUG] Using connection string: {debugConnStr}");

// Optimize connection string
var optimizedConnectionString = FoodX.Admin.Data.DatabaseConfiguration.OptimizeConnectionString(dbConnectionString);

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
        options.SignIn.RequireConfirmedAccount = !builder.Environment.IsDevelopment(); // True in production
        options.SignIn.RequireConfirmedEmail = !builder.Environment.IsDevelopment(); // True in production
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Register email sender (replace with real implementation in production)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Register role navigation service - commented out temporarily
// builder.Services.AddScoped<FoodX.Admin.Services.IRoleNavigationService, FoodX.Admin.Services.RoleNavigationService>();

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

// Add EmailServiceClient for Email Service microservice communication
builder.Services.AddHttpClient<FoodX.Admin.Services.EmailServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["EmailService:BaseUrl"] ?? "http://localhost:5257");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add CSV Import Service
builder.Services.AddScoped<FoodX.Admin.Services.ICsvImportService, FoodX.Admin.Services.CsvImportService>();

// Add Enhanced File Reading and Validation Services
builder.Services.AddScoped<FoodX.Admin.Services.IFileReaderService, FoodX.Admin.Services.FileReaderService>();
// Advanced validation service commented out temporarily
// builder.Services.AddScoped<FoodX.Admin.Services.IAdvancedValidationService, FoodX.Admin.Services.AdvancedValidationService>();
builder.Services.AddScoped<FoodX.Admin.Services.IImportHistoryService, FoodX.Admin.Services.ImportHistoryService>();
// builder.Services.AddScoped<FoodX.Admin.Services.IImportProgressService, FoodX.Admin.Services.ImportProgressService>();

// Add SignalR for real-time import progress
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 100 * 1024; // 100KB
});

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

// Add comprehensive health checks
builder.Services.AddFoodXHealthChecks(builder.Configuration);

var app = builder.Build();

// Create missing database tables if needed
using (var scope = app.Services.CreateScope())
{
    try
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Checking for missing database tables...");
        
        // Execute table creation
        await FoodX.Admin.CreateMissingTables.ExecuteMigration();

        // Apply RFQId migration
        await FoodX.Admin.RFQIdMigrationExtensions.ApplyRFQIdMigrationAsync(scope.ServiceProvider);

        logger.LogInformation("Database tables check completed.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not create missing tables. They may already exist or will be created on first use.");
    }
}

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

// Use Rate Limiting (must be before authentication)
app.UseRateLimiter();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Map API controllers
app.MapControllers();

// Configure Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new FoodX.Admin.Services.HangfireAuthorizationFilter() }
});

// Schedule recurring jobs
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<FoodX.Admin.Services.BackgroundJobs.IInvoiceGenerationJob>(
    "process-pending-invoices",
    job => job.ProcessPendingInvoices(),
    Cron.Hourly);

recurringJobManager.AddOrUpdate<FoodX.Admin.Services.BackgroundJobs.IInvoiceGenerationJob>(
    "generate-monthly-statements",
    job => job.GenerateMonthlyStatements(),
    Cron.Monthly(1, 9)); // First day of month at 9 AM

recurringJobManager.AddOrUpdate<FoodX.Admin.Services.BackgroundJobs.IInvoiceGenerationJob>(
    "send-payment-reminders",
    job => job.SendPaymentReminders(),
    Cron.Daily(10)); // Daily at 10 AM

recurringJobManager.AddOrUpdate<FoodX.Admin.Services.BackgroundJobs.IWorkflowAutomationJob>(
    "process-workflow-rules",
    job => job.ProcessWorkflowRules(),
    "*/15 * * * *"); // Every 15 minutes

recurringJobManager.AddOrUpdate<FoodX.Admin.Services.BackgroundJobs.IWorkflowAutomationJob>(
    "auto-confirm-orders",
    job => job.AutoConfirmOrders(),
    Cron.Hourly(30)); // Every hour at 30 minutes

recurringJobManager.AddOrUpdate<FoodX.Admin.Services.BackgroundJobs.IWorkflowAutomationJob>(
    "update-shipment-statuses",
    job => job.UpdateShipmentStatuses(),
    "*/30 * * * *"); // Every 30 minutes

recurringJobManager.AddOrUpdate<FoodX.Admin.Services.BackgroundJobs.IWorkflowAutomationJob>(
    "process-delayed-shipments",
    job => job.ProcessDelayedShipments(),
    Cron.Daily(8)); // Daily at 8 AM

// Map health checks endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Map health checks UI
app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Map SignalR hub for import progress - commented out temporarily
// app.MapHub<FoodX.Admin.Hubs.ImportProgressHub>("/hubs/import-progress");
// app.MapHub<FoodX.Admin.Hubs.NotificationHub>("/hubs/notifications");

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

// Apply buyer columns migration if needed
try
{
    Console.WriteLine("[INFO] Checking FoodXBuyers table schema...");
    FoodX.Admin.ApplyBuyerColumns.ExecuteMigration();
}
catch (Exception ex)
{
    Console.WriteLine($"[WARNING] Could not apply buyer columns migration: {ex.Message}");
    // Continue startup even if migration fails
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