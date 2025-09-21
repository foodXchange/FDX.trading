using Microsoft.AspNetCore.Identity;

namespace FoodX.Simple.Data
{
    public class UserSeeder
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserSeeder(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedUsersAsync()
        {
            // Create roles
            var roles = new[] { "Admin", "Buyer", "Supplier" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create test users
            var testUsers = new[]
            {
                new { Email = "admin@foodx.com", Password = "pass", Role = "Admin" },
                new { Email = "buyer@foodx.com", Password = "pass", Role = "Buyer" },
                new { Email = "supplier@foodx.com", Password = "pass", Role = "Supplier" }
            };

            foreach (var testUser in testUsers)
            {
                var user = await _userManager.FindByEmailAsync(testUser.Email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = testUser.Email,
                        Email = testUser.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, testUser.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, testUser.Role);
                    }
                }
            }
        }
    }
}