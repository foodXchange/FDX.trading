using Microsoft.AspNetCore.Mvc;
using FoodX.EmailService.Services;
using System.Text.Json;

namespace FoodX.EmailService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IEmailReceivingService _emailReceivingService;
    private readonly ILogger<WebhookController> _logger;
    private readonly IConfiguration _configuration;

    public WebhookController(
        IEmailReceivingService emailReceivingService,
        ILogger<WebhookController> logger,
        IConfiguration configuration)
    {
        _emailReceivingService = emailReceivingService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("sendgrid/inbound")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ReceiveSendGridEmail([FromForm] IFormCollection form)
    {
        try
        {
            _logger.LogInformation("Received SendGrid inbound email webhook");

            // Parse SendGrid inbound email from form data
            var inboundEmail = new SendGridInboundEmail
            {
                From = form["from"].ToString(),
                To = form["to"].ToString(),
                Subject = form["subject"].ToString(),
                Html = form["html"].ToString(),
                Text = form["text"].ToString(),
                Headers = form["headers"].ToString(),
                Envelope = form["envelope"].ToString(),
                Charsets = form["charsets"].ToString(),
                SpamScore = form["spam_score"].ToString(),
                SpamReport = form["spam_report"].ToString(),
                Dkim = form["dkim"].ToString(),
                Spf = form["spf"].ToString()
            };

            // Parse attachment count
            if (int.TryParse(form["attachments"].ToString(), out var attachmentCount))
            {
                inboundEmail.AttachmentCount = attachmentCount;
            }

            // Process attachments if any
            if (form.Files.Count > 0)
            {
                inboundEmail.Attachments = new Dictionary<string, Stream>();
                foreach (var file in form.Files)
                {
                    var stream = new MemoryStream();
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    inboundEmail.Attachments[file.FileName] = stream;
                }
            }

            // Process the email
            var email = await _emailReceivingService.ProcessInboundEmailAsync(inboundEmail);

            _logger.LogInformation($"Successfully processed inbound email with ID {email.Id}");

            // SendGrid expects a 200 OK response
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SendGrid inbound email");
            // Return 200 OK to prevent SendGrid from retrying
            // Log the error for investigation
            return Ok();
        }
    }

    [HttpPost("azure/email-events")]
    public async Task<IActionResult> ReceiveAzureEmailEvents([FromBody] JsonElement eventData)
    {
        try
        {
            _logger.LogInformation("Received Azure email event webhook");

            // Azure Event Grid sends events in an array
            if (eventData.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in eventData.EnumerateArray())
                {
                    await ProcessAzureEvent(item);
                }
            }
            else
            {
                await ProcessAzureEvent(eventData);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Azure email events");
            return StatusCode(500, new { Error = "Failed to process events" });
        }
    }

    private async Task ProcessAzureEvent(JsonElement eventItem)
    {
        try
        {
            var eventType = eventItem.GetProperty("eventType").GetString();

            // Handle subscription validation
            if (eventType == "Microsoft.EventGrid.SubscriptionValidationEvent")
            {
                var validationCode = eventItem.GetProperty("data").GetProperty("validationCode").GetString();
                Response.Headers["Content-Type"] = "application/json";
                await Response.WriteAsync(JsonSerializer.Serialize(new { validationResponse = validationCode }));
                return;
            }

            // Handle email events
            if (eventType?.StartsWith("Microsoft.Communication.EmailDelivery") == true)
            {
                var emailEvent = new AzureEmailEvent
                {
                    Id = eventItem.GetProperty("id").GetString() ?? "",
                    EventType = eventType,
                    Subject = eventItem.GetProperty("subject").GetString() ?? "",
                    EventTime = eventItem.GetProperty("eventTime").GetDateTime()
                };

                if (eventItem.TryGetProperty("data", out var data))
                {
                    emailEvent.Data = new EmailEventData
                    {
                        MessageId = data.GetProperty("messageId").GetString() ?? "",
                        From = data.GetProperty("from").GetString() ?? "",
                        To = data.GetProperty("to").GetString() ?? "",
                        Status = data.GetProperty("status").GetString() ?? ""
                    };

                    if (data.TryGetProperty("deliveryStatusDetails", out var details))
                    {
                        emailEvent.Data.DeliveryStatusDetails = details.GetString();
                    }
                }

                await _emailReceivingService.ProcessAzureEmailEventAsync(emailEvent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing individual Azure event");
            throw;
        }
    }

    [HttpPost("test")]
    public IActionResult TestWebhook([FromBody] object data)
    {
        _logger.LogInformation($"Test webhook received: {JsonSerializer.Serialize(data)}");
        return Ok(new { Success = true, Message = "Test webhook received successfully" });
    }
}