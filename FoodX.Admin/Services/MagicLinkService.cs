using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FoodX.Admin.Data;

namespace FoodX.Admin.Services
{
    public interface IMagicLinkService
    {
        Task<string> GenerateMagicLinkTokenAsync(string email);
        Task<bool> ValidateMagicLinkTokenAsync(string email, string token);
        Task<ApplicationUser?> GetUserByMagicLinkTokenAsync(string token);
        string GenerateMagicLinkUrl(string email, string token);
    }

    public class MagicLinkService : IMagicLinkService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MagicLinkService> _logger;
        private readonly ISendGridEmailService _emailService;

        public MagicLinkService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<MagicLinkService> logger,
            ISendGridEmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<string> GenerateMagicLinkTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Generate a secure token
            var tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            // Store the token with expiration (15 minutes)
            var magicLink = new MagicLinkToken
            {
                Token = token,
                UserId = user.Id,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            _context.MagicLinkTokens.Add(magicLink);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<bool> ValidateMagicLinkTokenAsync(string email, string token)
        {
            var magicLink = await _context.MagicLinkTokens
                .FirstOrDefaultAsync(m =>
                    m.Email == email &&
                    m.Token == token &&
                    !m.IsUsed &&
                    m.ExpiresAt > DateTime.UtcNow);

            if (magicLink == null)
            {
                return false;
            }

            // Mark token as used
            magicLink.IsUsed = true;
            magicLink.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ApplicationUser?> GetUserByMagicLinkTokenAsync(string token)
        {
            var magicLink = await _context.MagicLinkTokens
                .Include(m => m.User)
                .FirstOrDefaultAsync(m =>
                    m.Token == token &&
                    !m.IsUsed &&
                    m.ExpiresAt > DateTime.UtcNow);

            return magicLink?.User;
        }

        public string GenerateMagicLinkUrl(string email, string token)
        {
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5193";
            return $"{baseUrl}/Account/MagicLinkLogin?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        }
    }

    public class MagicLinkToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }

        // Navigation property
        public ApplicationUser User { get; set; } = null!;
    }
}