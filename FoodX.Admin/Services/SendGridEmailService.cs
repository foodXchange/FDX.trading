using SendGrid;
using SendGrid.Helpers.Mail;

namespace FoodX.Admin.Services
{
    public interface ISendGridEmailService
    {
        Task<bool> SendMagicLinkEmailAsync(string toEmail, string magicLinkUrl);
        Task<bool> SendInvitationEmailAsync(string toEmail, string invitationUrl, string inviterName);
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null);
    }

    public class SendGridEmailService : ISendGridEmailService
    {
        private readonly ISendGridClient? _sendGridClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(ISendGridClient? sendGridClient, IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _sendGridClient = sendGridClient;
            _configuration = configuration;
            _logger = logger;

            if (_sendGridClient != null)
            {
                _logger.LogInformation("SendGrid client injected successfully");
            }
            else
            {
                // Fallback: Try to get API key from configuration if client not injected
                var apiKey = _configuration["SendGridApiKey"] ?? _configuration["SendGrid:ApiKey"];

                if (!string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogInformation($"SendGrid API key found in configuration. Key starts with: {apiKey[..Math.Min(10, apiKey.Length)]}");
                    _sendGridClient = new SendGridClient(apiKey);
                }
                else
                {
                    _logger.LogWarning("SendGrid API key not configured. Emails will be logged only.");
                }
            }
        }

        public async Task<bool> SendMagicLinkEmailAsync(string toEmail, string magicLinkUrl)
        {
            const string subject = "Sign in to FoodX Trading Platform";

            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f5f5f5; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center; }}
        .header h1 {{ color: #ffffff; margin: 0; font-size: 28px; font-weight: 600; }}
        .content {{ padding: 40px 30px; }}
        .button-container {{ text-align: center; margin: 40px 0; }}
        .magic-button {{ display: inline-block; padding: 16px 40px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 16px; box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4); }}
        .magic-button:hover {{ box-shadow: 0 6px 20px rgba(102, 126, 234, 0.6); }}
        .security-notice {{ background-color: #f8f9fa; border-left: 4px solid #667eea; padding: 15px; margin: 30px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 30px; text-align: center; color: #666; font-size: 14px; }}
        .link-text {{ word-break: break-all; color: #667eea; font-size: 12px; margin: 20px 0; padding: 15px; background-color: #f8f9fa; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚀 FoodX Trading Platform</h1>
        </div>
        <div class='content'>
            <h2 style='color: #333; margin-bottom: 20px;'>Welcome back!</h2>
            <p style='font-size: 16px; color: #555;'>
                Click the button below to instantly sign in to your FoodX account. No password needed!
            </p>

            <div class='button-container'>
                <a href='{magicLinkUrl}' class='magic-button'>
                    ✨ Sign In with Magic Link
                </a>
            </div>

            <p style='color: #888; font-size: 14px; text-align: center;'>
                Or copy and paste this link into your browser:
            </p>
            <div class='link-text'>
                {magicLinkUrl}
            </div>

            <div class='security-notice'>
                <strong>🔒 Security Notice:</strong>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>This link expires in <strong>15 minutes</strong></li>
                    <li>It can only be used <strong>once</strong></li>
                    <li>Never share this link with anyone</li>
                </ul>
            </div>

            <p style='color: #888; font-size: 14px;'>
                If you didn't request this sign-in link, you can safely ignore this email.
            </p>
        </div>
        <div class='footer'>
            <p style='margin: 5px 0;'><strong>FoodX Trading Platform</strong></p>
            <p style='margin: 5px 0; color: #999;'>Revolutionizing B2B Food Trading</p>
            <p style='margin: 15px 0 5px 0; font-size: 12px; color: #aaa;'>
                © 2024 FoodX Trading. All rights reserved.
            </p>
        </div>
    </div>
</body>
</html>";

            var plainText = $@"
Sign in to FoodX Trading Platform

Welcome back!

Click this link to sign in instantly (no password needed):
{magicLinkUrl}

Security Notice:
• This link expires in 15 minutes
• It can only be used once
• Never share this link with anyone

If you didn't request this sign-in link, you can safely ignore this email.

FoodX Trading Platform
© 2024 FoodX Trading. All rights reserved.
";

            return await SendEmailAsync(toEmail, subject, htmlContent, plainText);
        }

        public async Task<bool> SendInvitationEmailAsync(string toEmail, string invitationUrl, string inviterName)
        {
            const string subject = "You're Invited to Join FoodX Trading Platform";

            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f5f5f5; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 40px 30px; text-align: center; }}
        .header h1 {{ color: #ffffff; margin: 0; font-size: 28px; font-weight: 600; }}
        .content {{ padding: 40px 30px; }}
        .invitation-box {{ background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%); border-radius: 12px; padding: 30px; margin: 30px 0; text-align: center; }}
        .button-container {{ text-align: center; margin: 40px 0; }}
        .accept-button {{ display: inline-block; padding: 16px 50px; background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 18px; box-shadow: 0 4px 15px rgba(17, 153, 142, 0.4); }}
        .accept-button:hover {{ box-shadow: 0 6px 20px rgba(17, 153, 142, 0.6); }}
        .benefits {{ background-color: #f8f9fa; padding: 25px; border-radius: 8px; margin: 30px 0; }}
        .benefit-item {{ display: flex; align-items: center; margin: 15px 0; }}
        .benefit-icon {{ font-size: 24px; margin-right: 15px; }}
        .footer {{ background-color: #f8f9fa; padding: 30px; text-align: center; color: #666; font-size: 14px; }}
        .link-text {{ word-break: break-all; color: #11998e; font-size: 12px; margin: 20px 0; padding: 15px; background-color: #f8f9fa; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome to FoodX!</h1>
        </div>
        <div class='content'>
            <div class='invitation-box'>
                <h2 style='color: #333; margin: 0 0 10px 0;'>You've Been Invited!</h2>
                <p style='font-size: 18px; color: #555; margin: 0;'>
                    <strong>{inviterName}</strong> has invited you to join<br>
                    the FoodX B2B Trading Platform
                </p>
            </div>

            <p style='font-size: 16px; color: #555;'>
                Join thousands of food industry professionals who are revolutionizing B2B food trading with our
                innovative platform.
            </p>

            <div class='benefits'>
                <h3 style='color: #333; margin-top: 0;'>What You'll Get:</h3>
                <div class='benefit-item'>
                    <span class='benefit-icon'>✅</span>
                    <span><strong>Passwordless Access:</strong> Sign in securely with magic links - no passwords to remember!</span>
                </div>
                <div class='benefit-item'>
                    <span class='benefit-icon'>🤝</span>
                    <span><strong>B2B Marketplace:</strong> Connect with verified suppliers and buyers</span>
                </div>
                <div class='benefit-item'>
                    <span class='benefit-icon'>📊</span>
                    <span><strong>Real-time Analytics:</strong> Track orders, inventory, and market trends</span>
                </div>
                <div class='benefit-item'>
                    <span class='benefit-icon'>🔒</span>
                    <span><strong>Secure Platform:</strong> Enterprise-grade security for your business</span>
                </div>
            </div>

            <div class='button-container'>
                <a href='{invitationUrl}' class='accept-button'>
                    Accept Invitation & Join
                </a>
            </div>

            <p style='color: #888; font-size: 14px; text-align: center;'>
                Or copy and paste this link into your browser:
            </p>
            <div class='link-text'>
                {invitationUrl}
            </div>

            <p style='color: #888; font-size: 14px; margin-top: 30px;'>
                <strong>Note:</strong> This invitation link expires in 7 days. If you need a new invitation,
                please contact {inviterName}.
            </p>
        </div>
        <div class='footer'>
            <p style='margin: 5px 0;'><strong>FoodX Trading Platform</strong></p>
            <p style='margin: 5px 0; color: #999;'>Revolutionizing B2B Food Trading</p>
            <p style='margin: 15px 0 5px 0; font-size: 12px; color: #aaa;'>
                © 2024 FoodX Trading. All rights reserved.
            </p>
        </div>
    </div>
</body>
</html>";

            var plainText = $@"
You're Invited to Join FoodX Trading Platform!

{inviterName} has invited you to join the FoodX B2B Trading Platform.

Join thousands of food industry professionals who are revolutionizing B2B food trading.

What You'll Get:
✅ Passwordless Access - Sign in securely with magic links
🤝 B2B Marketplace - Connect with verified suppliers and buyers
📊 Real-time Analytics - Track orders, inventory, and market trends
🔒 Secure Platform - Enterprise-grade security for your business

Accept your invitation here:
{invitationUrl}

Note: This invitation link expires in 7 days.

FoodX Trading Platform
© 2024 FoodX Trading. All rights reserved.
";

            return await SendEmailAsync(toEmail, subject, htmlContent, plainText);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null)
        {
            try
            {
                // If SendGrid is not configured, log the email
                if (_sendGridClient == null)
                {
                    _logger.LogInformation("📧 Email (Dev Mode - Not Sent):");
                    _logger.LogInformation($"To: {toEmail}");
                    _logger.LogInformation($"Subject: {subject}");

                    // Extract and log magic link if present - updated regex to handle both single and double quotes
                    var linkMatch = System.Text.RegularExpressions.Regex.Match(htmlContent, @"href=['""]([^'""]+)['""]");
                    if (linkMatch.Success)
                    {
                        _logger.LogInformation($"Magic Link: {linkMatch.Groups[1].Value}");

                        // Also save to file for easy access
                        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "magic-links.txt");
                        var logEntry = $@"
========================================
Time: {DateTime.Now}
To: {toEmail}
Subject: {subject}
Link: {linkMatch.Groups[1].Value}
========================================
";
                        await File.AppendAllTextAsync(logPath, logEntry);
                    }

                    return true;
                }

                // Send via SendGrid
                var from = new EmailAddress(
                    _configuration["SendGrid:FromEmail"] ?? "noreply@fdx.trading",
                    _configuration["SendGrid:FromName"] ?? "FoodX Trading"
                );
                var to = new EmailAddress(toEmail);

                var msg = MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    plainTextContent ?? "Please view this email in HTML format.",
                    htmlContent
                );

                // Optional: Add custom headers
                msg.AddHeader("X-Sent-By", "FoodX Trading Platform");

                var response = await _sendGridClient.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                    response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Email sent successfully to {toEmail} via SendGrid");
                    return true;
                }
                else
                {
                    var body = await response.Body.ReadAsStringAsync();
                    _logger.LogError($"SendGrid failed to send email. Status: {response.StatusCode}, Body: {body}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                return false;
            }
        }
    }
}