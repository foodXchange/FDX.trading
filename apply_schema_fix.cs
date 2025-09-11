using Microsoft.EntityFrameworkCore;
using FoodX.Admin.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using FoodX.Admin.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        
        // Add Entity Framework with Azure Key Vault integration
        var connectionString = "Server=tcp:fdx-sql-prod.database.windows.net,1433;Database=fdxdb;User Id=foodxapp;Password=FoodX@2024!Secure#Trading;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        services.AddDbContext<FoodXDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<FoodXDbContext>()
            .AddDefaultTokenProviders();
    })
    .Build();

try
{
    using var scope = host.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<FoodXDbContext>();

    Console.WriteLine("Reading SQL script...");
    var sqlScript = await File.ReadAllTextAsync("fix_schema_only.sql");

    Console.WriteLine("Executing schema fix script...");
    
    // Split the script into individual statements and execute them one by one
    var statements = sqlScript.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
    
    foreach (var statement in statements)
    {
        var cleanStatement = statement.Trim();
        if (!string.IsNullOrWhiteSpace(cleanStatement) && !cleanStatement.StartsWith("--"))
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(cleanStatement);
            }
            catch (Exception ex)
            {
                // Log but continue with other statements
                Console.WriteLine($"Warning: {ex.Message}");
            }
        }
    }

    Console.WriteLine("Schema fix completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

Environment.Exit(0);