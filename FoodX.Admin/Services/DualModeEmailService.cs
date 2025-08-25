using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;

namespace FoodX.Admin.Services
{
    /// <summary>
    /// Dual-mode email service that supports both SendGrid API and SMTP
    /// Automatically falls back to SMTP if API fails
    /// </summary>
    public class DualModeEmailService : ISendGridEmailService
    {
        private readonly ISendGridClient? _sendGridClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DualModeEmailService> _logger;
        private readonly string? _apiKey;
        private readonly bool _useSmtp;
        private readonly bool _useApi;

        public DualModeEmailService(ISendGridClient? sendGridClient, IConfiguration configuration, ILogger<DualModeEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _sendGridClient = sendGridClient;

            // Check which modes are enabled
            _useApi = _configuration.GetValue<bool>("SendGrid:UseApi", true);
            _useSmtp = _configuration.GetValue<bool>("SendGrid:UseSmtp", true);

            _logger.LogInformation($"Email Service Configuration: API={_useApi}, SMTP={_useSmtp}");

            if (_sendGridClient != null)
            {
                _logger.LogInformation("SendGrid client injected via dependency injection");
                _apiKey = _configuration["SendGridApiKey"] ?? _configuration["SendGrid:ApiKey"];
            }
            else
            {
                // Fallback: Get API key from Key Vault or configuration
                _apiKey = _configuration["SendGridApiKey"] ?? _configuration["SendGrid:ApiKey"];

                if (!string.IsNullOrEmpty(_apiKey) && _useApi)
                {
                    // Create SendGrid client with EU data residency (Poland is in EU)
                    var options = new SendGridClientOptions
                    {
                        ApiKey = _apiKey,
                        Host = "https://api.sendgrid.com", // Default host
                        Version = "v3"
                    };

                    // Use global endpoint (EU endpoint requires special account configuration)
                    // If you have EU data residency configured in SendGrid, uncomment the line below:
                    // options.Host = "https://api.eu.sendgrid.com";
                    _logger.LogInformation("SendGrid API client initialized with global endpoint");

                    _sendGridClient = new SendGridClient(options);
                }
            }
        }

        public async Task<bool> SendMagicLinkEmailAsync(string toEmail, string magicLinkUrl)
        {
            const string subject = "Sign in to FoodX Trading Platform";
            var htmlContent = GetMagicLinkHtmlTemplate(magicLinkUrl);
            var plainText = GetMagicLinkPlainText(magicLinkUrl);

            return await SendEmailAsync(toEmail, subject, htmlContent, plainText);
        }

        public async Task<bool> SendInvitationEmailAsync(string toEmail, string invitationUrl, string inviterName)
        {
            const string subject = "You're Invited to Join FoodX Trading Platform";
            var htmlContent = GetInvitationHtmlTemplate(invitationUrl, inviterName);
            var plainText = GetInvitationPlainText(invitationUrl, inviterName);

            return await SendEmailAsync(toEmail, subject, htmlContent, plainText);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null)
        {
            bool apiSuccess = false;
            bool smtpSuccess = false;

            // Try API first if enabled
            if (_useApi && _sendGridClient != null)
            {
                apiSuccess = await SendViaApiAsync(toEmail, subject, htmlContent, plainTextContent);
                if (apiSuccess)
                {
                    _logger.LogInformation($"✅ Email sent via API to {toEmail}");
                    return true;
                }
            }

            // Try SMTP if API failed or not enabled
            if (_useSmtp && !string.IsNullOrEmpty(_apiKey))
            {
                smtpSuccess = await SendViaSmtpAsync(toEmail, subject, htmlContent, plainTextContent);
                if (smtpSuccess)
                {
                    _logger.LogInformation($"✅ Email sent via SMTP to {toEmail}");
                    return true;
                }
            }

            // If both failed, log to file in development mode
            if (!apiSuccess && !smtpSuccess)
            {
                var environment = _configuration["ASPNETCORE_ENVIRONMENT"];
                if (environment == "Development")
                {
                    await LogEmailToFileAsync(toEmail, subject, htmlContent);
                    _logger.LogInformation($"📧 Email logged to file (Dev Mode): {toEmail}");
                    return true;
                }
            }

            _logger.LogError($"❌ Failed to send email to {toEmail} via all methods");
            return false;
        }

        private async Task<bool> SendViaApiAsync(string toEmail, string subject, string htmlContent, string? plainTextContent)
        {
            try
            {
                _logger.LogInformation($"Attempting to send email via SendGrid API to {toEmail}");
                
                var from = new EmailAddress(
                    _configuration["SendGrid:FromEmail"] ?? "noreply@fdx.trading",
                    _configuration["SendGrid:FromName"] ?? "FoodX Trading"
                );
                var to = new EmailAddress(toEmail);

                _logger.LogInformation($"From: {from.Email} ({from.Name}), To: {to.Email}");

                var msg = MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    plainTextContent ?? "Please view this email in HTML format.",
                    htmlContent
                );

                msg.AddHeader("X-Sent-By", "FoodX Trading Platform");
                msg.AddHeader("X-Method", "API");

                // Disable click tracking to avoid SendGrid rewriting URLs
                msg.TrackingSettings = new TrackingSettings
                {
                    ClickTracking = new ClickTracking { Enable = false, EnableText = false }
                };

                _logger.LogInformation("Sending email via SendGrid API...");
                var response = await _sendGridClient!.SendEmailAsync(msg);

                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation($"API Success: Email sent to {toEmail}, Status: {response.StatusCode}");
                    return true;
                }

                var body = await response.Body.ReadAsStringAsync();
                // Don't log as warning if it's a regional restriction - this is expected
                if (response.StatusCode == HttpStatusCode.Unauthorized && body.Contains("regional"))
                {
                    _logger.LogDebug($"API regional restriction, falling back to SMTP: {body}");
                }
                else
                {
                    _logger.LogWarning($"API Failed: Status={response.StatusCode}, Body={body}");
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"API Error: Failed to send email to {toEmail}");
                return false;
            }
        }

        private async Task<bool> SendViaSmtpAsync(string toEmail, string subject, string htmlContent, string? plainTextContent)
        {
            try
            {
                using var client = new SmtpClient("smtp.sendgrid.net", 587)
                {
                    Credentials = new NetworkCredential("apikey", _apiKey),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@fdx.trading";
                var fromName = _configuration["SendGrid:FromName"] ?? "FoodX Trading";

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = plainTextContent ?? "Please view this email in HTML format.",
                    IsBodyHtml = false  // Keep false because we're using AlternateViews
                };

                message.To.Add(new MailAddress(toEmail));
                message.Headers.Add("X-Sent-By", "FoodX Trading Platform");
                message.Headers.Add("X-Method", "SMTP");

                // Add both plain text and HTML views for proper multipart/alternative
                var plainView = AlternateView.CreateAlternateViewFromString(
                    plainTextContent ?? "Please view this email in HTML format.",
                    null,
                    System.Net.Mime.MediaTypeNames.Text.Plain
                );
                message.AlternateViews.Add(plainView);

                var htmlView = AlternateView.CreateAlternateViewFromString(
                    htmlContent,
                    null,
                    System.Net.Mime.MediaTypeNames.Text.Html
                );
                message.AlternateViews.Add(htmlView);

                await client.SendMailAsync(message);
                _logger.LogInformation($"SMTP Success: Email sent to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SMTP Error: Failed to send email to {toEmail}");
                return false;
            }
        }

        private async Task LogEmailToFileAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var linkMatch = System.Text.RegularExpressions.Regex.Match(htmlContent, @"href='([^']+)'");
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "magic-links.txt");

                var logEntry = $@"
========================================
Time: {DateTime.Now}
To: {toEmail}
Subject: {subject}
Link: {(linkMatch.Success ? linkMatch.Groups[1].Value : "N/A")}
========================================
";
                await File.AppendAllTextAsync(logPath, logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log email to file");
            }
        }

        private string GetMagicLinkHtmlTemplate(string magicLinkUrl)
        {
            return $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
    <title>Sign in to FoodX Trading Platform</title>
    <!--[if mso]>
    <noscript>
        <xml>
            <o:OfficeDocumentSettings>
                <o:PixelsPerInch>96</o:PixelsPerInch>
            </o:OfficeDocumentSettings>
        </xml>
    </noscript>
    <![endif]-->
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f7fa; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f4f7fa; padding: 40px 0;"">
        <tr>
            <td align=""center"">
                <!--[if (gte mso 9)|(IE)]>
                <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""600"">
                <tr>
                <td align=""center"" valign=""top"" width=""600"">
                <![endif]-->
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 48px 40px; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;"">FoodX</h1>
                            <p style=""margin: 8px 0 0 0; color: #e8e3ff; font-size: 14px; font-weight: 500; text-transform: uppercase; letter-spacing: 2px;"">Trading Platform</p>
                        </td>
                    </tr>

                    <!-- Main Content -->
                    <tr>
                        <td style=""padding: 48px 40px 24px 40px;"">
                            <h2 style=""margin: 0 0 16px 0; color: #1a1a1a; font-size: 28px; font-weight: 700; text-align: center;"">Welcome back! 👋</h2>
                            <p style=""margin: 0 0 32px 0; color: #4a5568; font-size: 16px; line-height: 1.6; text-align: center;"">
                                You're just one click away from accessing your FoodX account. No password needed!
                            </p>

                            <!-- Button Container -->
                            <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"" style=""padding: 8px 0 32px 0;"">
                                        <!--[if mso]>
                                        <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{magicLinkUrl}"" style=""height:52px;v-text-anchor:middle;width:280px;"" arcsize=""15%"" stroke=""f"" fillcolor=""#667eea"">
                                            <w:anchorlock/>
                                            <center>
                                        <![endif]-->
                                        <a href=""{magicLinkUrl}"" style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; display: inline-block; font-size: 16px; font-weight: 600; line-height: 52px; text-align: center; text-decoration: none; width: 280px; -webkit-text-size-adjust: none; border-radius: 8px; box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4); mso-hide: all;"">
                                            ✨ Sign In with Magic Link
                                        </a>
                                        <!--[if mso]>
                                            </center>
                                        </v:roundrect>
                                        <![endif]-->
                                    </td>
                                </tr>
                            </table>

                            <!-- Alternative Link Section -->
                            <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""margin: 0 0 32px 0;"">
                                <tr>
                                    <td style=""padding: 0 0 12px 0; text-align: center;"">
                                        <p style=""margin: 0; color: #718096; font-size: 13px;"">Having trouble with the button? Copy this link:</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""background-color: #f7fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 12px; word-break: break-all;"">
                                        <a href=""{magicLinkUrl}"" style=""color: #667eea; font-size: 12px; text-decoration: none; word-break: break-all;"">{magicLinkUrl}</a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Security Notice -->
                            <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                <tr>
                                    <td style=""background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%); border-radius: 8px; padding: 20px;"">
                                        <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                            <tr>
                                                <td width=""40"" valign=""top"" style=""padding-right: 12px; font-size: 24px;"">🔒</td>
                                                <td>
                                                    <p style=""margin: 0 0 8px 0; color: #92400e; font-weight: 600; font-size: 14px;"">Security Information</p>
                                                    <ul style=""margin: 0; padding-left: 20px; color: #92400e; font-size: 13px; line-height: 1.6;"">
                                                        <li>This link expires in <strong>15 minutes</strong></li>
                                                        <li>Can only be used <strong>once</strong></li>
                                                        <li>Never share this link with anyone</li>
                                                    </ul>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8fafc; padding: 32px 40px; text-align: center; border-top: 1px solid #e2e8f0;"">
                            <p style=""margin: 0 0 4px 0; color: #4a5568; font-weight: 600; font-size: 14px;"">FoodX Trading Platform</p>
                            <p style=""margin: 0 0 16px 0; color: #a0aec0; font-size: 12px;"">Revolutionizing B2B Food Trading</p>
                            <p style=""margin: 0; color: #cbd5e0; font-size: 11px;"">
                                If you didn't request this email, you can safely ignore it.<br/>
                                © 2024 FoodX Trading. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
                <!--[if (gte mso 9)|(IE)]>
                </td>
                </tr>
                </table>
                <![endif]-->
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string GetMagicLinkPlainText(string magicLinkUrl)
        {
            return $@"
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
        }

        private string GetInvitationHtmlTemplate(string invitationUrl, string inviterName)
        {
            return $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
    <title>You're Invited to FoodX Trading Platform</title>
    <!--[if mso]>
    <noscript>
        <xml>
            <o:OfficeDocumentSettings>
                <o:PixelsPerInch>96</o:PixelsPerInch>
            </o:OfficeDocumentSettings>
        </xml>
    </noscript>
    <![endif]-->
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f7fa; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f4f7fa; padding: 40px 0;"">
        <tr>
            <td align=""center"">
                <!--[if (gte mso 9)|(IE)]>
                <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""600"">
                <tr>
                <td align=""center"" valign=""top"" width=""600"">
                <![endif]-->
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 48px 40px; text-align: center;"">
                            <div style=""background: rgba(255,255,255,0.2); display: inline-block; padding: 12px 24px; border-radius: 50px; margin-bottom: 16px;"">
                                <span style=""font-size: 32px;"">🎉</span>
                            </div>
                            <h1 style=""margin: 0; color: #ffffff; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;"">Welcome to FoodX!</h1>
                            <p style=""margin: 8px 0 0 0; color: #c6f7e2; font-size: 16px; font-weight: 500;"">You've been invited to join our platform</p>
                        </td>
                    </tr>

                    <!-- Main Content -->
                    <tr>
                        <td style=""padding: 48px 40px 24px 40px;"">
                            <h2 style=""margin: 0 0 24px 0; color: #1a1a1a; font-size: 28px; font-weight: 700; text-align: center;"">You're In! 🚀</h2>

                            <!-- Inviter Info Card -->
                            <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""margin: 0 0 32px 0;"">
                                <tr>
                                    <td style=""background: linear-gradient(135deg, #e0f2fe 0%, #ddd6fe 100%); border-radius: 8px; padding: 24px; text-align: center;"">
                                        <p style=""margin: 0; color: #4338ca; font-size: 18px; line-height: 1.6;"">
                                            <strong style=""color: #1e293b; font-size: 20px;"">{inviterName}</strong><br/>
                                            has invited you to join the<br/>
                                            <strong style=""font-size: 20px;"">FoodX B2B Trading Platform</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin: 0 0 32px 0; color: #4a5568; font-size: 16px; line-height: 1.6; text-align: center;"">
                                Join thousands of businesses revolutionizing food trading with cutting-edge technology and seamless transactions.
                            </p>

                            <!-- Button Container -->
                            <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"" style=""padding: 8px 0 32px 0;"">
                                        <!--[if mso]>
                                        <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{invitationUrl}"" style=""height:56px;v-text-anchor:middle;width:320px;"" arcsize=""15%"" stroke=""f"" fillcolor=""#10b981"">
                                            <w:anchorlock/>
                                            <center>
                                        <![endif]-->
                                        <a href=""{invitationUrl}"" style=""background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: #ffffff; display: inline-block; font-size: 18px; font-weight: 600; line-height: 56px; text-align: center; text-decoration: none; width: 320px; -webkit-text-size-adjust: none; border-radius: 8px; box-shadow: 0 4px 15px rgba(16, 185, 129, 0.4); mso-hide: all;"">
                                            ✅ Accept Invitation & Get Started
                                        </a>
                                        <!--[if mso]>
                                            </center>
                                        </v:roundrect>
                                        <![endif]-->
                                    </td>
                                </tr>
                            </table>

                            <!-- Benefits Section -->
                            <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""margin: 0 0 32px 0;"">
                                <tr>
                                    <td style=""background-color: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 8px; padding: 24px;"">
                                        <p style=""margin: 0 0 16px 0; color: #15803d; font-weight: 600; font-size: 16px; text-align: center;"">What You'll Get Access To:</p>
                                        <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                            <tr>
                                                <td style=""padding: 8px 0;"">
                                                    <span style=""color: #16a34a; font-size: 16px; margin-right: 8px;"">✓</span>
                                                    <span style=""color: #374151; font-size: 14px;"">Real-time B2B food trading marketplace</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0;"">
                                                    <span style=""color: #16a34a; font-size: 16px; margin-right: 8px;"">✓</span>
                                                    <span style=""color: #374151; font-size: 14px;"">Secure payment processing</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0;"">
                                                    <span style=""color: #16a34a; font-size: 16px; margin-right: 8px;"">✓</span>
                                                    <span style=""color: #374151; font-size: 14px;"">Advanced analytics and reporting</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style=""padding: 8px 0;"">
                                                    <span style=""color: #16a34a; font-size: 16px; margin-right: 8px;"">✓</span>
                                                    <span style=""color: #374151; font-size: 14px;"">24/7 customer support</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <!-- Alternative Link Section -->
                            <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""margin: 0 0 24px 0;"">
                                <tr>
                                    <td style=""padding: 0 0 12px 0; text-align: center;"">
                                        <p style=""margin: 0; color: #718096; font-size: 13px;"">Having trouble with the button? Copy this link:</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""background-color: #f7fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 12px; word-break: break-all;"">
                                        <a href=""{invitationUrl}"" style=""color: #10b981; font-size: 12px; text-decoration: none; word-break: break-all;"">{invitationUrl}</a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Expiry Notice -->
                            <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                <tr>
                                    <td style=""text-align: center; padding: 16px; background-color: #fef2f2; border: 1px solid #fecaca; border-radius: 6px;"">
                                        <p style=""margin: 0; color: #dc2626; font-size: 14px; font-weight: 500;"">
                                            ⏰ This invitation expires in <strong>7 days</strong>
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8fafc; padding: 32px 40px; text-align: center; border-top: 1px solid #e2e8f0;"">
                            <p style=""margin: 0 0 4px 0; color: #4a5568; font-weight: 600; font-size: 14px;"">FoodX Trading Platform</p>
                            <p style=""margin: 0 0 16px 0; color: #a0aec0; font-size: 12px;"">Revolutionizing B2B Food Trading</p>
                            <p style=""margin: 0; color: #cbd5e0; font-size: 11px;"">
                                This invitation was sent to you by {inviterName}.<br/>
                                © 2024 FoodX Trading. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
                <!--[if (gte mso 9)|(IE)]>
                </td>
                </tr>
                </table>
                <![endif]-->
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string GetInvitationPlainText(string invitationUrl, string inviterName)
        {
            return $@"
You're Invited to Join FoodX Trading Platform!

{inviterName} has invited you to join the FoodX B2B Trading Platform.

Accept your invitation here:
{invitationUrl}

This invitation expires in 7 days.

FoodX Trading Platform
© 2024 FoodX Trading. All rights reserved.
";
        }
    }
}