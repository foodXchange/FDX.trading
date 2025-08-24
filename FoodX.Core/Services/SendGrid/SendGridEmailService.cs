using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodX.Core.Services.SendGrid;

public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailService(
        ISendGridClient sendGridClient,
        IConfiguration configuration,
        ILogger<SendGridEmailService> logger)
    {
        _sendGridClient = sendGridClient;
        _configuration = configuration;
        _logger = logger;
        _fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@fdx.trading";
        _fromName = _configuration["SendGrid:FromName"] ?? "FoodX Platform";
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, null, htmlContent);
            
            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {To}", to);
                return true;
            }
            
            _logger.LogWarning("Failed to send email. Status: {Status}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendTemplateEmailAsync(string to, string templateId, object templateData)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleTemplateEmail(from, toAddress, templateId, templateData);
            
            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Template email sent successfully to {To}", to);
                return true;
            }
            
            _logger.LogWarning("Failed to send template email. Status: {Status}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template email to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string htmlContent)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var tos = recipients.Select(r => new EmailAddress(r)).ToList();
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, null, htmlContent);
            
            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Bulk email sent successfully to {Count} recipients", recipients.Count);
                return true;
            }
            
            _logger.LogWarning("Failed to send bulk email. Status: {Status}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk email");
            return false;
        }
    }

    public async Task<bool> SendMagicLinkAsync(string email, string magicLink)
    {
        var htmlContent = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2>Your Magic Link</h2>
                <p>Click the link below to sign in:</p>
                <a href='{magicLink}' style='display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Sign In</a>
                <p style='margin-top: 20px; color: #666;'>This link will expire in 15 minutes.</p>
                <p style='color: #666;'>If you didn't request this, please ignore this email.</p>
            </div>";

        return await SendEmailAsync(email, "Your FoodX Sign-In Link", htmlContent);
    }
}