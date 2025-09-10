using Microsoft.EntityFrameworkCore;
using FoodX.EmailService.Data;
using FoodX.EmailService.Models;

namespace FoodX.EmailService.Services;

/// <summary>
/// Background service that periodically cleans up old deleted emails
/// </summary>
public class EmailCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Run daily
    private readonly int _retentionDays = 30; // Keep deleted emails for 30 days

    public EmailCleanupService(
        IServiceProvider serviceProvider,
        ILogger<EmailCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);
                
                if (stoppingToken.IsCancellationRequested)
                    break;

                await PerformCleanup();
            }
            catch (TaskCanceledException)
            {
                // Service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email cleanup");
            }
        }

        _logger.LogInformation("Email Cleanup Service stopped");
    }

    private async Task PerformCleanup()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmailDbContext>();

        try
        {
            _logger.LogInformation("Starting email cleanup process");

            var cutoffDate = DateTime.UtcNow.AddDays(-_retentionDays);

            // Find emails marked for deletion that are older than retention period
            var emailsToDelete = await dbContext.Emails
                .Where(e => e.IsDeleted && e.DeletedAt != null && e.DeletedAt < cutoffDate)
                .Include(e => e.Attachments)
                .ToListAsync();

            if (emailsToDelete.Any())
            {
                _logger.LogInformation($"Found {emailsToDelete.Count} emails to permanently delete");

                // Delete associated attachments first
                var attachmentIds = emailsToDelete.SelectMany(e => e.Attachments != null ? e.Attachments : new List<EmailAttachment>())
                    .Select(a => a.Id)
                    .ToList();

                if (attachmentIds.Any())
                {
                    var attachments = await dbContext.EmailAttachments
                        .Where(a => attachmentIds.Contains(a.Id))
                        .ToListAsync();
                    
                    dbContext.EmailAttachments.RemoveRange(attachments);
                    _logger.LogInformation($"Removing {attachments.Count} attachments");
                }

                // Handle thread cleanup
                var threadIds = emailsToDelete
                    .Where(e => e.ThreadId.HasValue)
                    .Select(e => e.ThreadId!.Value)
                    .Distinct()
                    .ToList();

                foreach (var threadId in threadIds)
                {
                    var thread = await dbContext.EmailThreads
                        .Include(t => t.Emails)
                        .FirstOrDefaultAsync(t => t.Id == threadId);

                    if (thread != null)
                    {
                        // Check if there will be any emails left in the thread after deletion
                        var remainingEmails = thread.Emails
                            .Where(e => !emailsToDelete.Any(ed => ed.Id == e.Id))
                            .ToList();

                        if (!remainingEmails.Any())
                        {
                            // Delete the thread if no emails remain
                            dbContext.EmailThreads.Remove(thread);
                            _logger.LogInformation($"Removing empty thread {threadId}");
                        }
                        else
                        {
                            // Update thread metadata
                            thread.EmailCount = remainingEmails.Count;
                            thread.LastActivityAt = remainingEmails.Max(e => e.CreatedAt);
                            thread.HasUnread = remainingEmails.Any(e => e.ReadAt == null && e.Direction == "Inbound");
                        }
                    }
                }

                // Remove the emails
                dbContext.Emails.RemoveRange(emailsToDelete);

                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Successfully deleted {emailsToDelete.Count} emails");
            }
            else
            {
                _logger.LogInformation("No emails found for cleanup");
            }

            // Clean up orphaned threads (threads with no emails)
            var orphanedThreads = await dbContext.EmailThreads
                .Where(t => !t.Emails.Any())
                .ToListAsync();

            if (orphanedThreads.Any())
            {
                dbContext.EmailThreads.RemoveRange(orphanedThreads);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Cleaned up {orphanedThreads.Count} orphaned threads");
            }

            // Clean up old draft emails (older than 7 days)
            var oldDrafts = await dbContext.Emails
                .Where(e => e.Status == "Draft" && e.CreatedAt < DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            if (oldDrafts.Any())
            {
                dbContext.Emails.RemoveRange(oldDrafts);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation($"Cleaned up {oldDrafts.Count} old draft emails");
            }

            _logger.LogInformation("Email cleanup process completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email cleanup");
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Cleanup Service is starting");
        
        // Perform initial cleanup on startup
        await PerformCleanup();
        
        await base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Cleanup Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}