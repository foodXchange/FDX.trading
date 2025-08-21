using Microsoft.AspNetCore.Mvc;
using FoodX.Admin.Services;

namespace FoodX.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEmailController : ControllerBase
    {
        private readonly ISendGridEmailService _emailService;
        private readonly ILogger<TestEmailController> _logger;
        private readonly IConfiguration _configuration;

        public TestEmailController(
            ISendGridEmailService emailService,
            ILogger<TestEmailController> logger,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("test-html")]
        public async Task<IActionResult> TestHtmlEmail([FromQuery] string? email = null)
        {
            try
            {
                var testEmail = email ?? "udi@fdx.trading";
                var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5193";
                var testMagicLink = $"{baseUrl}/Account/MagicLinkLogin?email={Uri.EscapeDataString(testEmail)}&token=TEST_TOKEN_12345";

                _logger.LogInformation($"Sending test HTML email to {testEmail}");
                
                var success = await _emailService.SendMagicLinkEmailAsync(testEmail, testMagicLink);
                
                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Test HTML email sent to {testEmail}",
                        note = "Check your inbox - the email should display as formatted HTML with a button, not raw code"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Failed to send email",
                        note = "Check application logs for details"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email");
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        [HttpGet("verify-config")]
        public IActionResult VerifyConfiguration()
        {
            var hasApiKey = !string.IsNullOrEmpty(_configuration["SendGridApiKey"] ?? _configuration["SendGrid:ApiKey"]);
            var useApi = _configuration.GetValue<bool>("SendGrid:UseApi", true);
            var useSmtp = _configuration.GetValue<bool>("SendGrid:UseSmtp", true);
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
            
            return Ok(new
            {
                hasApiKey,
                useApi,
                useSmtp,
                environment,
                fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@fdx.trading",
                fromName = _configuration["SendGrid:FromName"] ?? "FoodX Trading",
                baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5193"
            });
        }
    }
}