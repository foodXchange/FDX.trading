using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodX.Admin.Models
{
    [Table("BuyerRequests")]
    public class BuyerRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BuyerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string InputType { get; set; } = "Text"; // Image, URL, or Text

        [Required]
        public string InputContent { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Analyzed, Approved, Failed

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BuyerId")]
        public virtual FoodXBuyer? Buyer { get; set; }

        public virtual ICollection<AIAnalysisResult> AnalysisResults { get; set; } = new List<AIAnalysisResult>();

        // Helper properties
        [NotMapped]
        public bool IsProcessed => Status == "Analyzed" || Status == "Approved";

        [NotMapped]
        public bool HasAnalysis => AnalysisResults?.Any() == true;

        [NotMapped]
        public AIAnalysisResult? LatestAnalysis => AnalysisResults?.OrderByDescending(a => a.ProcessedAt).FirstOrDefault();
    }

    public static class RequestStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Analyzed = "Analyzed";
        public const string Approved = "Approved";
        public const string Failed = "Failed";
    }

    public static class InputTypes
    {
        public const string Image = "Image";
        public const string URL = "URL";
        public const string Text = "Text";
    }
}