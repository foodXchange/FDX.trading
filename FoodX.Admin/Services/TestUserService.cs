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
    }
}