using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FoodX.Admin.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace FoodX.Admin
{
    public class CreateAdminUser
    {
        public static async Task CreateAdminAccounts(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure roles exist
            string[] roleNames = { "SuperAdmin", "Admin", "Buyer", "Supplier", "Expert", "Agent" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"Created role: {roleName}");
                }
            }

            // Create admin@foodx.com
            var adminEmail = "admin@foodx.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    CompanyName = "FoodX Platform"
                };

                var result = await userManager.CreateAsync(adminUser, "FoodX@Admin2024!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                    Console.WriteLine($"Created admin user: {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Reset password if user exists
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var result = await userManager.ResetPasswordAsync(adminUser, token, "FoodX@Admin2024!");

                if (result.Succeeded)
                {
                    // Ensure user has SuperAdmin role
                    if (!await userManager.IsInRoleAsync(adminUser, "SuperAdmin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                    }
                    Console.WriteLine($"Reset password for: {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Create system@foodx.com
            var systemEmail = "system@foodx.com";
            var systemUser = await userManager.FindByEmailAsync(systemEmail);

            if (systemUser == null)
            {
                systemUser = new ApplicationUser
                {
                    UserName = systemEmail,
                    Email = systemEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Account",
                    CompanyName = "FoodX Platform"
                };

                var result = await userManager.CreateAsync(systemUser, "System@FoodX2024!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(systemUser, "SuperAdmin");
                    Console.WriteLine($"Created system user: {systemEmail}");
                }
                else
                {
                    Console.WriteLine($"Failed to create system user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Reset password if user exists
                var token = await userManager.GeneratePasswordResetTokenAsync(systemUser);
                var result = await userManager.ResetPasswordAsync(systemUser, token, "System@FoodX2024!");

                if (result.Succeeded)
                {
                    // Ensure user has SuperAdmin role
                    if (!await userManager.IsInRoleAsync(systemUser, "SuperAdmin"))
                    {
                        await userManager.AddToRoleAsync(systemUser, "SuperAdmin");
                    }
                    Console.WriteLine($"Reset password for: {systemEmail}");
                }
                else
                {
                    Console.WriteLine($"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            Console.WriteLine("\nAdmin accounts ready:");
            Console.WriteLine("Email: admin@foodx.com | Password: FoodX@Admin2024!");
            Console.WriteLine("Email: system@foodx.com | Password: System@FoodX2024!");
        }
    }
}