using Microsoft.EntityFrameworkCore;
using FoodX.EmailService.Models;

namespace FoodX.EmailService.Data;

public class EmailDbContext : DbContext
{
    public EmailDbContext(DbContextOptions<EmailDbContext> options)
        : base(options)
    {
    }

    public DbSet<Email> Emails { get; set; }
    public DbSet<EmailThread> EmailThreads { get; set; }
    public DbSet<EmailAttachment> EmailAttachments { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<EmailTemplateUsage> EmailTemplateUsages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Email entity configuration
        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasIndex(e => e.MessageId).IsUnique(false);
            entity.HasIndex(e => e.FromEmail);
            entity.HasIndex(e => e.ToEmail);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Direction);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.BuyerId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Category);

            entity.HasOne(e => e.Thread)
                .WithMany(t => t.Emails)
                .HasForeignKey(e => e.ThreadId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // EmailThread entity configuration
        modelBuilder.Entity<EmailThread>(entity =>
        {
            entity.HasIndex(e => e.LastActivityAt);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.BuyerId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsArchived);
            entity.HasIndex(e => e.HasUnread);
        });

        // EmailAttachment entity configuration
        modelBuilder.Entity<EmailAttachment>(entity =>
        {
            entity.HasIndex(e => e.EmailId);
            
            entity.HasOne(a => a.Email)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.EmailId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EmailTemplate entity configuration
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.Category, e.IsActive });
        });

        // EmailTemplateUsage entity configuration
        modelBuilder.Entity<EmailTemplateUsage>(entity =>
        {
            entity.HasIndex(e => e.TemplateId);
            entity.HasIndex(e => e.EmailId);
            entity.HasIndex(e => e.UsedAt);
            
            entity.HasOne(u => u.Template)
                .WithMany(t => t.Usages)
                .HasForeignKey(u => u.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}