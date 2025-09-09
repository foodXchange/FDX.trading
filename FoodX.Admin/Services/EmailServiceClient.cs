using System.Net.Http.Json;
using System.Text.Json;

namespace FoodX.Admin.Services;

public interface IEmailServiceClient
{
    Task<EmailSendResponse> SendEmailAsync(EmailSendRequest request);
    Task<EmailSendResponse> SendReplyAsync(int originalEmailId, EmailSendRequest request);
    Task<EmailSendResponse> ResendEmailAsync(int emailId);
    Task<EmailInboxResponse> GetInboxAsync(string userEmail, int page = 1, int pageSize = 20, string? folder = null, string? category = null, string? search = null);
    Task<List<EmailDto>> GetRecentSentEmailsAsync(string userEmail, int count = 5);
    Task<EmailThreadDto> GetThreadAsync(int threadId);
    Task<bool> MarkAsReadAsync(int emailId);
    Task<EmailActionResponse> DeleteEmailAsync(int emailId);
}

public class EmailServiceClient : IEmailServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailServiceClient> _logger;
    private readonly IConfiguration _configuration;

    public EmailServiceClient(HttpClient httpClient, ILogger<EmailServiceClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        
        // Configure base address if not already set
        if (_httpClient.BaseAddress == null)
        {
            var baseUrl = _configuration["EmailService:BaseUrl"] ?? "https://localhost:7001";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<EmailSendResponse> SendEmailAsync(EmailSendRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/email/send", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<EmailSendResponse>();
            return result ?? new EmailSendResponse { Success = false, Message = "No response received" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending email");
            return new EmailSendResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<EmailSendResponse> SendReplyAsync(int originalEmailId, EmailSendRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/email/reply/{originalEmailId}", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<EmailSendResponse>();
            return result ?? new EmailSendResponse { Success = false, Message = "No response received" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error sending reply to email {originalEmailId}");
            return new EmailSendResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<EmailSendResponse> ResendEmailAsync(int emailId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/email/resend/{emailId}", null);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<EmailSendResponse>();
            return result ?? new EmailSendResponse { Success = false, Message = "No response received" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error resending email {emailId}");
            return new EmailSendResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<EmailInboxResponse> GetInboxAsync(string userEmail, int page = 1, int pageSize = 20, string? folder = null, string? category = null, string? search = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"userEmail={Uri.EscapeDataString(userEmail)}",
                $"page={page}",
                $"pageSize={pageSize}"
            };
            
            if (!string.IsNullOrWhiteSpace(folder))
                queryParams.Add($"folder={Uri.EscapeDataString(folder)}");
            if (!string.IsNullOrWhiteSpace(category))
                queryParams.Add($"category={Uri.EscapeDataString(category)}");
            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            
            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"api/email/inbox?{queryString}");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<EmailInboxResponse>();
            return result ?? new EmailInboxResponse { Success = false, Emails = new List<EmailDto>() };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error getting inbox for {userEmail}");
            return new EmailInboxResponse { Success = false, Emails = new List<EmailDto>() };
        }
    }
    
    public async Task<List<EmailDto>> GetRecentSentEmailsAsync(string userEmail, int count = 5)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/email/sent/recent?userEmail={Uri.EscapeDataString(userEmail)}&count={count}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EmailInboxResponse>();
                return result?.Emails ?? new List<EmailDto>();
            }
            
            return new List<EmailDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting recent sent emails for {userEmail}");
            return new List<EmailDto>();
        }
    }
    
    public async Task<EmailActionResponse> DeleteEmailAsync(int emailId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/email/{emailId}");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<EmailActionResponse>();
            return result ?? new EmailActionResponse { Success = false, Message = "No response received" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error deleting email {emailId}");
            return new EmailActionResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<EmailThreadDto> GetThreadAsync(int threadId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/email/thread/{threadId}");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<EmailThreadDto>();
            return result ?? new EmailThreadDto();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error getting thread {threadId}");
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(int emailId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/email/mark-read/{emailId}", null);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Error marking email {emailId} as read");
            return false;
        }
    }
}

// DTOs for Email Service communication
public class EmailSendRequest
{
    public string To { get; set; } = string.Empty;
    public string? From { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? PlainTextBody { get; set; }
    public string? Category { get; set; }
    public int? SupplierId { get; set; }
    public int? BuyerId { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, string>? MetaData { get; set; }
    public List<EmailAttachmentDto>? Attachments { get; set; }
    public bool IsHighPriority { get; set; } = false;
}

public class EmailAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Base64Content { get; set; } = string.Empty;
    public bool IsInline { get; set; } = false;
    public string? ContentId { get; set; }
}

public class EmailSendResponse
{
    public bool Success { get; set; }
    public int EmailId { get; set; }
    public int? ThreadId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class EmailInboxResponse
{
    public bool Success { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public List<EmailDto> Emails { get; set; } = new();
}

public class EmailActionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class EmailDto
{
    public int Id { get; set; }
    public string? MessageId { get; set; }
    public int? ThreadId { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? PlainTextBody { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public List<EmailAttachmentResponse>? Attachments { get; set; }
}

public class EmailAttachmentResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string? DownloadUrl { get; set; }
    public bool IsInline { get; set; }
}

public class EmailThreadDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public List<string> Participants { get; set; } = new();
    public DateTime LastActivityAt { get; set; }
    public int EmailCount { get; set; }
    public bool HasUnread { get; set; }
    public string? Category { get; set; }
    public List<EmailDto> Emails { get; set; } = new();
}