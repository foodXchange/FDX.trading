using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FoodX.Admin.Data;

namespace FoodX.Admin.Services
{
    public class TestUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestUserService> _logger;

        public TestUserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<TestUserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ResetTestUserPasswordsAsync()
        {
            var testPassword = "TestPass123!";
            var testUsers = await _context.Users
                .Where(u => u.Email != null && u.Email.EndsWith("@test.fdx.trading"))
                .ToListAsync();

            var results = new List<string>();

            foreach (var user in testUsers)
            {
                try
                {
                    // Generate password reset token
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    // Reset password
                    var result = await _userManager.ResetPasswordAsync(user, token, testPassword);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Password reset successful for {user.Email}");
                        results.Add($"✓ {user.Email}");
                    }
                    else
                    {
                        _logger.LogWarning($"Password reset failed for {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        results.Add($"✗ {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error resetting password for {user.Email}");
                    results.Add($"✗ {user.Email}: {ex.Message}");
                }
            }

            _logger.LogInformation($"Password reset complete. Results:\n{string.Join("\n", results)}");
            return results.All(r => r.StartsWith("✓"));
        }

        public async Task<bool> EnsureRolesExistAsync()
        {
            string[] roles = { "SuperAdmin", "Admin", "Buyer", "Supplier", "Agent", "Expert" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        _logger.LogError($"Failed to create role {role}");
                        return false;
                    }
                    _logger.LogInformation($"Created role: {role}");
                }
            }

            return true;
        }

        public async Task<Dictionary<string, List<string>>> GetUsersByRoleAsync()
        {
            var usersByRole = new Dictionary<string, List<string>>();
            var roles = await _roleManager.Roles.ToListAsync();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                usersByRole[role.Name!] = usersInRole.Select(u => u.Email ?? "No Email").ToList();
            }

            return usersByRole;
        }

        public async Task<bool> TestLoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"User not found: {email}");
                return false;
            }

            var result = await _userManager.CheckPasswordAsync(user, password);
            if (result)
            {
                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation($"Login successful for {email}");
            }
            else
            {
                _logger.LogWarning($"Invalid password for {email}");
            }

            return result;
        }

        public async Task<bool> CreateSuperAdminAsync(string email)
        {
            try
            {
                // Ensure SuperAdmin role exists
                if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
                    _logger.LogInformation("Created SuperAdmin role");
                }

                // Find or create user
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        CompanyName = "FDX Trading",
                        FirstName = "Udi",
                        LastName = "Admin"
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        _logger.LogError($"Failed to create user {email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                        return false;
                    }
                    _logger.LogInformation($"Created new user {email}");
                }

                // Remove existing roles and add SuperAdmin
                var existingRoles = await _userManager.GetRolesAsync(user);
                if (existingRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, existingRoles);
                    _logger.LogInformation($"Removed existing roles from {email}: {string.Join(", ", existingRoles)}");
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "SuperAdmin");
                if (!roleResult.Succeeded)
                {
                    _logger.LogError($"Failed to add SuperAdmin role to {email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    return false;
                }

                _logger.LogInformation($"Successfully assigned SuperAdmin role to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating SuperAdmin user {email}");
                return false;
            }
        }
    }
}