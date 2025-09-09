using Microsoft.EntityFrameworkCore;
using SendGrid.Extensions.DependencyInjection;
using FoodX.EmailService.Data;
using FoodX.EmailService.Services;
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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure CORS for FoodX.Admin
builder.Services.AddCors(options =>
{
    options.AddPolicy("FoodXPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5195",  // FoodX.Admin local
                "https://localhost:5195",
                "http://localhost:5196",  // Alternative port
                "https://localhost:5196"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("EmailDb") 
    ?? builder.Configuration["EmailDb:ConnectionString"]
    ?? "Server=(localdb)\\mssqllocaldb;Database=FoodXEmails;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<EmailDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure SendGrid
var sendGridApiKey = builder.Configuration["SendGridApiKey"] 
    ?? builder.Configuration["SendGrid:ApiKey"]
    ?? Environment.GetEnvironmentVariable("SENDGRID_API_KEY");

if (!string.IsNullOrEmpty(sendGridApiKey))
{
    builder.Services.AddSendGrid(options =>
    {
        options.ApiKey = sendGridApiKey;
    });
}
else
{
    Console.WriteLine("[WARNING] SendGrid API key not configured");
}

// Register Email Services
builder.Services.AddScoped<IEmailSendingService, EmailSendingService>();
builder.Services.AddScoped<IEmailReceivingService, EmailReceivingService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EmailDbContext>("database");

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Application Insights can be added later if needed
// if (builder.Environment.IsProduction())
// {
//     builder.Logging.AddApplicationInsights();
// }

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("FoodXPolicy");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Create database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailDbContext>();
        dbContext.Database.EnsureCreated();
        Console.WriteLine("[INFO] Database initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Failed to initialize database: {ex.Message}");
    }
}

// Log startup information
Console.WriteLine($"[INFO] Email Service starting on {builder.Environment.EnvironmentName} environment");
Console.WriteLine($"[INFO] SendGrid configured: {!string.IsNullOrEmpty(sendGridApiKey)}");
Console.WriteLine($"[INFO] Database: {connectionString?.Split(';')[1] ?? "Not configured"}");

app.Run();
