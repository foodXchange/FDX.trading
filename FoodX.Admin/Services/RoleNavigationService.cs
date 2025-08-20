using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using FoodX.Admin.Data;
using System.Security.Claims;

namespace FoodX.Admin.Services
{
    public interface IRoleNavigationService
    {
        Task<string> GetRoleBasedDashboardUrl(ClaimsPrincipal user);
        Task<string> GetDefaultDashboardForRole(string role);
        Task<List<string>> GetUserRoles(ClaimsPrincipal user);
    }

    public class RoleNavigationService : IRoleNavigationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RoleNavigationService> _logger;

        public RoleNavigationService(
            UserManager<ApplicationUser> userManager,
            ILogger<RoleNavigationService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<string> GetRoleBasedDashboardUrl(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return "/";
            }

            var roles = await GetUserRoles(user);

            // Priority order for multiple roles
            if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
            {
                return "/dashboard/admin";
            }
            else if (roles.Contains("Agent"))
            {
                return "/dashboard/agent";
            }
            else if (roles.Contains("Expert"))
            {
                return "/dashboard/expert";
            }
            else if (roles.Contains("Supplier"))
            {
                return "/dashboard/supplier";
            }
            else if (roles.Contains("Buyer"))
            {
                return "/dashboard/buyer";
            }

            // Default fallback
            return "/";
        }

        public Task<string> GetDefaultDashboardForRole(string role)
        {
            var dashboard = role?.ToLower() switch
            {
                "admin" or "superadmin" => "/dashboard/admin",
                "buyer" => "/dashboard/buyer",
                "supplier" => "/dashboard/supplier",
                "expert" => "/dashboard/expert",
                "agent" => "/dashboard/agent",
                _ => "/"
            };

            return Task.FromResult(dashboard);
        }

        public async Task<List<string>> GetUserRoles(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return [];
            }

            // Try to get roles from claims first (faster)
            var rolesFromClaims = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (rolesFromClaims.Any())
            {
                return rolesFromClaims;
            }

            // If no roles in claims, get from UserManager
            var email = user.Identity.Name;
            if (string.IsNullOrEmpty(email))
            {
                return [];
            }

            try
            {
                var appUser = await _userManager.FindByEmailAsync(email);
                if (appUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(appUser);
                    return roles.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles for {Email}", email);
            }

            return [];
        }
    }
}