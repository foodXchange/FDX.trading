using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace FoodX.Admin.Data;

public static class DatabaseConfiguration
{
    public static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, string connectionString, bool isDevelopment)
    {
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            // Enable retry logic for transient failures
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: new[] 
                { 
                    // Add specific SQL error numbers for retry
                    -2, // Timeout
                    -1, // Connection broken
                    2, // Network error
                    14, // Connection not open
                    64, // Connection was successfully established but then broken
                    233, // Connection initialization error
                    20186, // Replication error
                    40197, // Service error
                    40501, // Service busy
                    40613, // Database unavailable
                    49918, // Cannot process request
                    49919, // Cannot process create/update request
                    49920, // Cannot process delete request
                    4060, // Cannot open database
                    18456 // Login failed
                });
            
            // Set command timeout
            sqlOptions.CommandTimeout(60);
            
            // Use row number for paging (better performance for large datasets)
            sqlOptions.UseCompatibilityLevel(120);
            
            // Enable query splitting for better performance with includes
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        // Configure for optimal performance
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        
        // Enable sensitive data logging only in development
        if (isDevelopment)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
        
        // Enable service provider caching
        optionsBuilder.EnableServiceProviderCaching();
        
        // Configure warning behaviors
        optionsBuilder.ConfigureWarnings(warnings =>
        {
            // Throw on any query that might result in client evaluation
            warnings.Throw(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning);
        });
    }

    public static string OptimizeConnectionString(string baseConnectionString)
    {
        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            // Connection pooling settings
            MinPoolSize = 5,
            MaxPoolSize = 100,
            Pooling = true,
            
            // Connection resiliency
            ConnectTimeout = 30,
            ConnectRetryCount = 3,
            ConnectRetryInterval = 10,
            
            // Performance optimizations
            MultipleActiveResultSets = true,
            ApplicationName = "FoodX.Admin",
            
            // Security
            Encrypt = true,
            TrustServerCertificate = false,
            
            // Application intent for read scale-out scenarios
            ApplicationIntent = ApplicationIntent.ReadWrite
        };

        return builder.ConnectionString;
    }
}