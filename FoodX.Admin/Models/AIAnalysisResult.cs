using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace FoodX.Admin.Models
{
    [Table("AIAnalysisResults")]
    public class AIAnalysisResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public string AnalysisData { get; set; } = "{}"; // JSON data

        public decimal? ConfidenceScore { get; set; }

        public int? ProcessingTime { get; set; } // in milliseconds

        [MaxLength(50)]
        public string? AIProvider { get; set; } // OpenAI, Azure, etc.

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("RequestId")]
        public virtual BuyerRequest? Request { get; set; }

        // Helper methods to work with JSON data
        [NotMapped]
        public ProductAnalysis? ParsedAnalysis
        {
            get
            {
                try
                {
                    return JsonSerializer.Deserialize<ProductAnalysis>(AnalysisData);
                }
                catch
                {
                    return null;
                }
            }
        }

        public void SetAnalysisData(ProductAnalysis analysis)
        {
            AnalysisData = JsonSerializer.Serialize(analysis, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
    }

    // Structured class for AI analysis results
    public class ProductAnalysis
    {
        public ProductIdentification? ProductIdentification { get; set; }
        public DetailedDescription? DetailedDescription { get; set; }
        public TechnicalSpecifications? TechnicalSpecifications { get; set; }
        public CategoryClassification? CategoryClassification { get; set; }
        public CommonAttributes? CommonAttributes { get; set; }
        public MarketContext? MarketContext { get; set; }
    }

    public class ProductIdentification
    {
        public string DetectedProduct { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string? BrandReference { get; set; }
        public string GenericName { get; set; } = string.Empty;
    }

    public class DetailedDescription
    {
        public string Summary { get; set; } = string.Empty;
        public List<string> KeyCharacteristics { get; set; } = new();
    }

    public class TechnicalSpecifications
    {
        public string? ProductDimensions { get; set; }
        public string? Composition { get; set; }
        public string? ColorProfile { get; set; }
        public string? TextureProfile { get; set; }
        public Dictionary<string, string> AdditionalSpecs { get; set; } = new();
    }

    public class CategoryClassification
    {
        public string PrimaryCategory { get; set; } = string.Empty;
        public string SecondaryCategory { get; set; } = string.Empty;
        public string SpecificType { get; set; } = string.Empty;
        public List<string> AlternativeNames { get; set; } = new();
    }

    public class CommonAttributes
    {
        public List<string> TypicalIngredients { get; set; } = new();
        public string? FlavorNotes { get; set; }
        public List<string> UsageOccasions { get; set; } = new();
        public string? ShelfLife { get; set; }
        public List<string> Certifications { get; set; } = new();
    }

    public class MarketContext
    {
        public List<string> CommonBrands { get; set; } = new();
        public string? TypicalPackaging { get; set; }
        public string? MarketPositioning { get; set; }
        public string? PriceSegment { get; set; }
    }
}