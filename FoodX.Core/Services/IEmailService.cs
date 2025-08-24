namespace FoodX.Core.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlContent);
    Task<bool> SendTemplateEmailAsync(string to, string templateId, object templateData);
    Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string htmlContent);
    Task<bool> SendMagicLinkAsync(string email, string magicLink);
}