using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FoodX.Admin.Services
{
    public class RoleNavigationService : IRoleNavigationService
    {
        private readonly UserManager<Data.ApplicationUser> _userManager;
        private readonly ILogger<RoleNavigationService> _logger;

        public RoleNavigationService(
            UserManager<Data.ApplicationUser> userManager,
            ILogger<RoleNavigationService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<string> GetRoleBasedDashboardUrl(ClaimsPrincipal user)
        {
            var roles = await GetUserRoles(user);
            
            if (roles.Contains("Admin") || roles.Contains("SystemAdmin"))
                return "/admin/dashboard";
            else if (roles.Contains("Supplier"))
                return "/portal/supplier/dashboard";
            else if (roles.Contains("Buyer"))
                return "/portal/buyer/dashboard";
            else if (roles.Contains("Agent"))
                return "/portal/agent/dashboard";
            else
                return "/dashboard";
        }

        public async Task<List<string>> GetUserRoles(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return new List<string>();

            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
                return new List<string>();

            var roles = await _userManager.GetRolesAsync(appUser);
            return roles.ToList();
        }

        public async Task<Dictionary<string, List<NavigationItem>>> GetNavigationMenuForUser(ClaimsPrincipal user)
        {
            var roles = await GetUserRoles(user);
            var menu = new Dictionary<string, List<NavigationItem>>();

            // Main menu items
            var mainItems = new List<NavigationItem>();
            
            if (roles.Contains("Admin") || roles.Contains("SystemAdmin"))
            {
                mainItems.Add(new NavigationItem
                {
                    Title = "Dashboard",
                    Url = "/admin/dashboard",
                    Icon = "Dashboard"
                });
                mainItems.Add(new NavigationItem
                {
                    Title = "Users",
                    Url = "/admin/users",
                    Icon = "People"
                });
                mainItems.Add(new NavigationItem
                {
                    Title = "Import",
                    Url = "/admin/import",
                    Icon = "FileUpload",
                    Children = new List<NavigationItem>
                    {
                        new NavigationItem { Title = "Suppliers", Url = "/admin/import/suppliers", Icon = "Store" },
                        new NavigationItem { Title = "Buyers", Url = "/admin/import/buyers", Icon = "Business" },
                        new NavigationItem { Title = "Products", Url = "/admin/import/products", Icon = "Category" }
                    }
                });
            }

            if (roles.Contains("Supplier"))
            {
                mainItems.Add(new NavigationItem
                {
                    Title = "Dashboard",
                    Url = "/portal/supplier/dashboard",
                    Icon = "Dashboard"
                });
                mainItems.Add(new NavigationItem
                {
                    Title = "Products",
                    Url = "/portal/supplier/products",
                    Icon = "Category"
                });
                mainItems.Add(new NavigationItem
                {
                    Title = "RFQs",
                    Url = "/portal/supplier/rfqs",
                    Icon = "RequestQuote"
                });
                mainItems.Add(new NavigationItem
                {
                    Title = "Orders",
                    Url = "/portal/orders",
                    Icon = "ShoppingCart"
                });
            }

            if (roles.Contains("Buyer"))
            {
                mainItems.Add(new NavigationItem
                {
                    Title = "Dashboard",
                    Url = "/portal/buyer/dashboard",
                    Icon = "Dashboard"
                });
                mainItems.Add(new NavigationItem
                {
                    Title = "AI Search",
                    Url = "/portal/buyer/ai-search",
                    Icon = "Search"
                });
                mainItems.Add(new NavigationItem
                {
                    Title = "RFQs",
                    Url = "/portal/buyer/rfqs",
                    Icon = "RequestQuote"
                });
                mainItems.Add(new NavigationItem
                {
                    Title = "Orders",
                    Url = "/portal/orders",
                    Icon = "ShoppingCart"
                });
            }

            // Common items for all authenticated users
            mainItems.Add(new NavigationItem
            {
                Title = "Messages",
                Url = "/support/email/inbox",
                Icon = "Email"
            });

            menu["main"] = mainItems;
            return menu;
        }

        public async Task<List<QuickAction>> GetQuickActionsForUser(ClaimsPrincipal user)
        {
            var roles = await GetUserRoles(user);
            var actions = new List<QuickAction>();

            if (roles.Contains("Admin") || roles.Contains("SystemAdmin"))
            {
                actions.Add(new QuickAction
                {
                    Title = "Import Data",
                    Icon = "FileUpload",
                    Url = "/admin/import/suppliers",
                    Color = "success",
                    Description = "Import suppliers, buyers, or products"
                });
                actions.Add(new QuickAction
                {
                    Title = "Manage Users",
                    Icon = "People",
                    Url = "/admin/users",
                    Color = "primary",
                    Description = "Add or manage user accounts"
                });
            }

            if (roles.Contains("Supplier"))
            {
                actions.Add(new QuickAction
                {
                    Title = "Add Product",
                    Icon = "Add",
                    Url = "/portal/supplier/products/new",
                    Color = "success",
                    Description = "Add a new product to your catalog"
                });
                actions.Add(new QuickAction
                {
                    Title = "View RFQs",
                    Icon = "RequestQuote",
                    Url = "/portal/supplier/rfqs",
                    Color = "info",
                    Description = "Check new RFQ requests"
                });
            }

            if (roles.Contains("Buyer"))
            {
                actions.Add(new QuickAction
                {
                    Title = "Search Products",
                    Icon = "Search",
                    Url = "/portal/buyer/ai-search",
                    Color = "primary",
                    Description = "Find products using AI search"
                });
                actions.Add(new QuickAction
                {
                    Title = "Create RFQ",
                    Icon = "Add",
                    Url = "/portal/buyer/rfq/new",
                    Color = "success",
                    Description = "Request quotes from suppliers"
                });
            }

            return actions;
        }
    }
}