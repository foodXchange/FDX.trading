using System;

namespace FoodX.Core.Configuration
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; } = new();
        public EmailSettings Email { get; set; } = new();
        public AzureSettings Azure { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
        public PortalUrls PortalUrls { get; set; } = new();
        public FeatureFlags Features { get; set; } = new();
    }

    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; } = string.Empty;
        public string FdxDb { get; set; } = string.Empty;
        public string RedisConnection { get; set; } = string.Empty;
    }

    public class EmailSettings
    {
        public string Provider { get; set; } = "SendGrid";
        public SendGridSettings SendGrid { get; set; } = new();
        public string FromEmail { get; set; } = "noreply@fdx.trading";
        public string FromName { get; set; } = "FoodX Platform";
        public string Mode { get; set; } = "Development";
    }

    public class SendGridSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public bool UseApi { get; set; } = true;
        public bool UseSmtp { get; set; } = false;
        public string SmtpHost { get; set; } = "smtp.sendgrid.net";
        public int SmtpPort { get; set; } = 587;
    }

    public class AzureSettings
    {
        public string KeyVaultUrl { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public AzureOpenAISettings OpenAI { get; set; } = new();
        public string StorageConnectionString { get; set; } = string.Empty;
    }

    public class AzureOpenAISettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string EmbeddingDeployment { get; set; } = "text-embedding-ada-002";
        public int Dimensions { get; set; } = 1536;
    }

    public class SecuritySettings
    {
        public JwtSettings Jwt { get; set; } = new();
        public int PasswordMinLength { get; set; } = 8;
        public bool RequireTwoFactor { get; set; } = false;
        public int LockoutMinutes { get; set; } = 30;
        public int MaxFailedAttempts { get; set; } = 5;
        public CorsSettings Cors { get; set; } = new();
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "https://fdx.trading";
        public string Audience { get; set; } = "fdx-platform";
        public int ExpirationMinutes { get; set; } = 60;
        public int RefreshExpirationDays { get; set; } = 7;
    }

    public class CorsSettings
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public bool AllowCredentials { get; set; } = true;
        public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        public string[] AllowedHeaders { get; set; } = new[] { "*" };
    }

    public class PortalUrls
    {
        public string AdminPortal { get; set; } = "http://localhost:5193";
        public string BuyerPortal { get; set; } = "http://localhost:5000";
        public string SupplierPortal { get; set; } = "http://localhost:5001";
        public string Marketplace { get; set; } = "http://localhost:5002";

        // Production URLs
        public PortalProductionUrls Production { get; set; } = new();
    }

    public class PortalProductionUrls
    {
        public string AdminPortal { get; set; } = "https://admin.fdx.trading";
        public string BuyerPortal { get; set; } = "https://buyer.fdx.trading";
        public string SupplierPortal { get; set; } = "https://supplier.fdx.trading";
        public string Marketplace { get; set; } = "https://fdx.trading";
    }

    public class FeatureFlags
    {
        public bool EnableVectorSearch { get; set; } = false;
        public bool EnableMagicLink { get; set; } = true;
        public bool EnableTwoFactor { get; set; } = false;
        public bool EnableApiRateLimiting { get; set; } = true;
        public bool EnableDetailedLogging { get; set; } = true;
        public bool EnableMaintenanceMode { get; set; } = false;
        public string MaintenanceMessage { get; set; } = "The platform is undergoing maintenance. Please check back later.";
    }
}