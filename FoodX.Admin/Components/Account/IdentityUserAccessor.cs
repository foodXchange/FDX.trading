using Microsoft.AspNetCore.Identity;
using FoodX.Admin.Data;

namespace FoodX.Admin.Components.Account;

internal sealed class IdentityUserAccessor(UserManager<ApplicationUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.User == null)
        {
            redirectManager.RedirectToWithStatus("Account/Login", "Error: Please log in to access this page.", context);
            return null!;
        }

        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user!;
    }
}
