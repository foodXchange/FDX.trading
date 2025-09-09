using FoodX.EmailService.Models;
using FoodX.EmailService.Models.DTOs;

namespace FoodX.EmailService.Services;

public interface IEmailSendingService
{
    Task<Email> SendEmailAsync(EmailRequest request);
    Task<Email> SendTemplateEmailAsync(string templateId, EmailRequest request);
    Task<bool> ResendEmailAsync(int emailId);
    Task<List<Email>> SendBulkEmailsAsync(List<EmailRequest> requests);
    Task<Email> SendReplyAsync(int originalEmailId, EmailRequest replyRequest);
}