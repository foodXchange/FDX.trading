using System.Security.Claims;

namespace FoodX.Admin.Services;

public enum PortalMode
{
    Admin,
    Supplier,
    Buyer,
    Marketplace
}

public interface IPortalContextService
{
    PortalMode CurrentMode { get; }
    bool IsImpersonating { get; }
    ClaimsPrincipal? OriginalUser { get; }
    ClaimsPrincipal? ImpersonatedUser { get; }
    DateTime? ImpersonationStartTime { get; }
    
    event EventHandler<PortalMode>? PortalModeChanged;
    event EventHandler<ImpersonationEventArgs>? ImpersonationChanged;
    
    void SetPortalMode(PortalMode mode);
    Task<bool> StartImpersonation(string userId);
    void EndImpersonation();
    TimeSpan? GetImpersonationTimeRemaining();
}

public class ImpersonationEventArgs : EventArgs
{
    public bool IsImpersonating { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}

public class PortalContextService : IPortalContextService
{
    private readonly ILogger<PortalContextService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _impersonationTimeout = TimeSpan.FromMinutes(30);
    
    private PortalMode _currentMode = PortalMode.Admin;
    private ClaimsPrincipal? _originalUser;
    private ClaimsPrincipal? _impersonatedUser;
    private DateTime? _impersonationStartTime;
    
    public PortalMode CurrentMode => _currentMode;
    public bool IsImpersonating => _impersonatedUser != null;
    public ClaimsPrincipal? OriginalUser => _originalUser;
    public ClaimsPrincipal? ImpersonatedUser => _impersonatedUser;
    public DateTime? ImpersonationStartTime => _impersonationStartTime;
    
    public event EventHandler<PortalMode>? PortalModeChanged;
    public event EventHandler<ImpersonationEventArgs>? ImpersonationChanged;
    
    public PortalContextService(ILogger<PortalContextService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public void SetPortalMode(PortalMode mode)
    {
        if (_currentMode != mode)
        {
            _logger.LogInformation($"Switching portal mode from {_currentMode} to {mode}");
            _currentMode = mode;
            PortalModeChanged?.Invoke(this, mode);
        }
    }
    
    public async Task<bool> StartImpersonation(string userId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<FoodX.Admin.Data.ApplicationUser>>();
            var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            
            var userToImpersonate = await userManager.FindByIdAsync(userId);
            if (userToImpersonate == null)
            {
                _logger.LogWarning($"User not found for impersonation: {userId}");
                return false;
            }
            
            // Store original user
            _originalUser = httpContextAccessor.HttpContext?.User;
            
            // Create claims for impersonated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userToImpersonate.Id),
                new Claim(ClaimTypes.Name, userToImpersonate.Email ?? string.Empty),
                new Claim(ClaimTypes.Email, userToImpersonate.Email ?? string.Empty),
                new Claim("ImpersonationActive", "true"),
                new Claim("OriginalUserId", _originalUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty)
            };
            
            // Add user roles
            var roles = await userManager.GetRolesAsync(userToImpersonate);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var identity = new ClaimsIdentity(claims, "Impersonation");
            _impersonatedUser = new ClaimsPrincipal(identity);
            _impersonationStartTime = DateTime.UtcNow;
            
            _logger.LogInformation($"Started impersonation of user {userToImpersonate.Email} by {_originalUser?.Identity?.Name}");
            
            // Determine appropriate portal mode based on user's primary role
            if (roles.Contains("Supplier"))
                SetPortalMode(PortalMode.Supplier);
            else if (roles.Contains("Buyer"))
                SetPortalMode(PortalMode.Buyer);
            else
                SetPortalMode(PortalMode.Admin);
            
            ImpersonationChanged?.Invoke(this, new ImpersonationEventArgs
            {
                IsImpersonating = true,
                UserId = userToImpersonate.Id,
                UserName = $"{userToImpersonate.FirstName} {userToImpersonate.LastName}",
                UserEmail = userToImpersonate.Email
            });
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting impersonation");
            return false;
        }
    }
    
    public void EndImpersonation()
    {
        if (!IsImpersonating)
            return;
        
        _logger.LogInformation($"Ended impersonation of user {_impersonatedUser?.Identity?.Name}");
        
        _impersonatedUser = null;
        _impersonationStartTime = null;
        _originalUser = null;
        
        // Return to admin mode
        SetPortalMode(PortalMode.Admin);
        
        ImpersonationChanged?.Invoke(this, new ImpersonationEventArgs
        {
            IsImpersonating = false
        });
    }
    
    public TimeSpan? GetImpersonationTimeRemaining()
    {
        if (!IsImpersonating || !_impersonationStartTime.HasValue)
            return null;
        
        var elapsed = DateTime.UtcNow - _impersonationStartTime.Value;
        var remaining = _impersonationTimeout - elapsed;
        
        if (remaining <= TimeSpan.Zero)
        {
            EndImpersonation();
            return TimeSpan.Zero;
        }
        
        return remaining;
    }
}