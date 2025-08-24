using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendGrid.Extensions.DependencyInjection;
using FoodX.Core.Configuration;
using FoodX.Core.Services;
using FoodX.Core.Services.Cache;
using FoodX.Core.Services.Configuration;
using FoodX.Core.Services.SendGrid;

namespace FoodX.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFoodXCore(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure AppSettings
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        
        // Add Memory Cache
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Add SendGrid
        var sendGridApiKey = configuration["SendGridApiKey"] ?? configuration["SendGrid:ApiKey"];
        if (!string.IsNullOrEmpty(sendGridApiKey))
        {
            services.AddSendGrid(options =>
            {
                options.ApiKey = sendGridApiKey;
            });
            services.AddScoped<IEmailService, SendGridEmailService>();
        }

        return services;
    }

    public static IServiceCollection AddFoodXRepositories(this IServiceCollection services)
    {
        // Repository pattern will be registered by each module with their specific DbContext
        return services;
    }
}