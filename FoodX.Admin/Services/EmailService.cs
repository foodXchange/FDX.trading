using System.Net.Mail;
using System.Net;

namespace FoodX.Admin.Services
{
    public interface IEmailService
    {
        Task SendMagicLinkEmailAsync(string email, string magicLinkUrl);
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMagicLinkEmailAsync(string email, string magicLinkUrl)
        {
            var subject = "Your FoodX Login Link";
            var htmlMessage = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>Sign in to FoodX B2B Platform</h2>
                        <p>Hello,</p>
                        <p>You requested a magic link to sign in to your FoodX account. Click the button below to sign in:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{magicLinkUrl}' 
                               style='display: inline-block; padding: 12px 30px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                Sign In to FoodX
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>Or copy and paste this link into your browser:</p>
                        <p style='color: #007bff; word-break: break-all; font-size: 12px;'>{magicLinkUrl}</p>
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                        <p style='color: #999; font-size: 12px;'>
                            <strong>Security Notice:</strong><br>
                            • This link will expire in 15 minutes<br>
                            • This link can only be used once<br>
                            • If you didn't request this, please ignore this email
                        </p>
                        <p style='color: #999; font-size: 12px; margin-top: 20px;'>
                            Best regards,<br>
                            The FoodX Team
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                // For development, just log the email
                if (_configuration["Email:Mode"] == "Development" || string.IsNullOrEmpty(_configuration["Email:SmtpServer"]))
                {
                    _logger.LogInformation("Development Mode - Email would be sent:");
                    _logger.LogInformation($"To: {email}");
                    _logger.LogInformation($"Subject: {subject}");
                    _logger.LogInformation($"Magic Link: {ExtractMagicLink(htmlMessage)}");

                    // Also write to a file for easy testing
                    var emailLogPath = Path.Combine(Directory.GetCurrentDirectory(), "magic-links.txt");
                    var logEntry = $@"
========================================
Time: {DateTime.Now}
To: {email}
Subject: {subject}
Magic Link: {ExtractMagicLink(htmlMessage)}
========================================
";
                    await File.AppendAllTextAsync(emailLogPath, logEntry);
                    return;
                }

                // Production email sending (configure SMTP settings in appsettings.json)
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@foodx.com";
                var fromName = _configuration["Email:FromName"] ?? "FoodX Platform";

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
                throw;
            }
        }

        private string ExtractMagicLink(string htmlMessage)
        {
            var startIndex = htmlMessage.IndexOf("href='") + 6;
            var endIndex = htmlMessage.IndexOf("'", startIndex);
            if (startIndex > 5 && endIndex > startIndex)
            {
                return htmlMessage[startIndex..endIndex];
            }
            return "Link not found";
        }
    }
}