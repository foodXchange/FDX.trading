using System;
using System.Threading.Tasks;
using FoodX.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace FoodX.Core.Services.Configuration
{
    public interface IConfigurationService
    {
        AppSettings GetSettings();
        Task<string> GetSecretAsync(string key);
        string GetConnectionString(string name = "DefaultConnection");
        bool IsFeatureEnabled(string featureName);
        string GetPortalUrl(string portalName);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly AppSettings _settings;
        private readonly IConfiguration _configuration;

        public ConfigurationService(IOptions<AppSettings> settings, IConfiguration configuration)
        {
            _settings = settings.Value;
            _configuration = configuration;
        }

        public AppSettings GetSettings()
        {
            return _settings;
        }

        public async Task<string> GetSecretAsync(string key)
        {
            // First try environment variable
            var envValue = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(envValue))
                return envValue;

            // Then try configuration
            var configValue = _configuration[key];
            if (!string.IsNullOrEmpty(configValue))
                return configValue;

            // TODO: Add Azure Key Vault integration here
            // if (!string.IsNullOrEmpty(_settings.Azure.KeyVaultUrl))
            // {
            //     return await GetFromKeyVault(key);
            // }

            return await Task.FromResult(string.Empty);
        }

        public string GetConnectionString(string name = "DefaultConnection")
        {
            return name switch
            {
                "DefaultConnection" => _settings.ConnectionStrings.DefaultConnection,
                "FdxDb" => _settings.ConnectionStrings.FdxDb,
                "Redis" => _settings.ConnectionStrings.RedisConnection,
                _ => _configuration.GetConnectionString(name) ?? string.Empty
            };
        }

        public bool IsFeatureEnabled(string featureName)
        {
            return featureName?.ToLower() switch
            {
                "vectorsearch" => _settings.Features.EnableVectorSearch,
                "magiclink" => _settings.Features.EnableMagicLink,
                "twofactor" => _settings.Features.EnableTwoFactor,
                "ratelimiting" => _settings.Features.EnableApiRateLimiting,
                "detailedlogging" => _settings.Features.EnableDetailedLogging,
                "maintenance" => _settings.Features.EnableMaintenanceMode,
                _ => false
            };
        }

        public string GetPortalUrl(string portalName)
        {
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            if (isDevelopment)
            {
                return portalName?.ToLower() switch
                {
                    "admin" => _settings.PortalUrls.AdminPortal,
                    "buyer" => _settings.PortalUrls.BuyerPortal,
                    "supplier" => _settings.PortalUrls.SupplierPortal,
                    "marketplace" => _settings.PortalUrls.Marketplace,
                    _ => _settings.PortalUrls.AdminPortal
                };
            }
            else
            {
                return portalName?.ToLower() switch
                {
                    "admin" => _settings.PortalUrls.Production.AdminPortal,
                    "buyer" => _settings.PortalUrls.Production.BuyerPortal,
                    "supplier" => _settings.PortalUrls.Production.SupplierPortal,
                    "marketplace" => _settings.PortalUrls.Production.Marketplace,
                    _ => _settings.PortalUrls.Production.AdminPortal
                };
            }
        }
    }
}