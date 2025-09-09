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
    }
}