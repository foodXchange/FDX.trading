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
        public PackagingDetails? PackagingDetails { get; set; }
        public LabelingInformation? LabelingInformation { get; set; }
        public VisualElements? VisualElements { get; set; }
        public ProductAttributes? ProductAttributes { get; set; }
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

    public class PackagingDetails
    {
        public string? PackageType { get; set; } // box, wrapper, bottle, can, pouch, tray, etc.
        public string? Material { get; set; } // plastic, cardboard, metal, glass, composite, etc.
        public string? Dimensions { get; set; } // Length x Width x Height
        public string? NetWeight { get; set; } // in grams
        public string? NetWeightOz { get; set; } // in ounces
        public string? NetVolume { get; set; } // in ml
        public string? NetVolumeFlOz { get; set; } // in fluid ounces
        public string? UnitsPerPackage { get; set; } // e.g., "4 cookies", "12 pieces"
        public string? ServingSize { get; set; } // e.g., "2 cookies (28g)"
        public string? ServingsPerContainer { get; set; } // e.g., "6"
        public List<string> PackagingComponents { get; set; } = new(); // outer box, inner tray, wrapper, etc.
        public bool IsResealable { get; set; }
        public bool IsRecyclable { get; set; }
        public string? RecyclingCode { get; set; } // recycling symbol number
    }

    public class LabelingInformation
    {
        public string? ProductNameOnLabel { get; set; } // exact product name as shown
        public string? BrandName { get; set; } // exact brand name as shown
        public string? Manufacturer { get; set; }
        public string? ManufacturerCompany { get; set; } // parent company name
        public string? ManufacturerWebsite { get; set; } // company website URL
        public string? BrandWebsite { get; set; } // specific brand website if different
        public string? CountryOfOrigin { get; set; }
        public List<string> Languages { get; set; } = new(); // languages used on packaging
        public string? Barcode { get; set; } // barcode number if visible
        public string? SKU { get; set; } // product SKU if visible
        public List<string> IngredientsText { get; set; } = new(); // full ingredients list as written
        public Dictionary<string, string> NutritionalInfo { get; set; } = new(); // nutrition facts
        public List<string> Allergens { get; set; } = new(); // allergen warnings
        public List<string> CertificationMarks { get; set; } = new(); // Kosher, Halal, Organic, etc.
        public string? BestBeforeDate { get; set; } // if visible
        public string? StorageInstructions { get; set; }
        public string? PreparationInstructions { get; set; }
        public List<string> Warnings { get; set; } = new(); // safety warnings
        public List<string> MarketingClaims { get; set; } = new(); // "Original", "New", "Improved", etc.
        public string? ContactInformation { get; set; } // manufacturer contact details
        public List<string> RegulatoryText { get; set; } = new(); // legal/regulatory text
    }

    public class VisualElements
    {
        public List<string> PrimaryColors { get; set; } = new(); // main colors used
        public string? LogoDescription { get; set; } // description of brand logo
        public List<string> ImageDescriptions { get; set; } = new(); // product images on package
        public string? DesignStyle { get; set; } // modern, classic, minimalist, etc.
        public bool HasWindowDisplay { get; set; } // transparent window showing product
        public List<string> SpecialEffects { get; set; } = new(); // metallic, glossy, matte, embossed, etc.
        public string? PackageShape { get; set; } // rectangular, cylindrical, irregular, etc.
    }

    public class ProductAttributes
    {
        // Dietary Certifications
        public bool? IsKosher { get; set; }
        public string? KosherCertification { get; set; } // OU, OK, Star-K, etc.
        public bool? IsHalal { get; set; }
        public string? HalalCertification { get; set; } // certification body name
        
        // Allergen Information
        public bool? IsGlutenFree { get; set; }
        public bool? IsNutFree { get; set; }
        public bool? IsDairyFree { get; set; }
        public bool? IsEggFree { get; set; }
        public bool? IsSoyFree { get; set; }
        public bool? IsShellFishFree { get; set; }
        public List<string> ContainsAllergens { get; set; } = new();
        public List<string> MayContainAllergens { get; set; } = new(); // cross-contamination warnings
        
        // Sugar & Sweeteners
        public bool? IsSugarFree { get; set; }
        public bool? IsNoSugarAdded { get; set; }
        public bool? IsDiabeticFriendly { get; set; }
        public List<string> SugarSubstitutes { get; set; } = new(); // stevia, aspartame, sucralose, etc.
        public string? TotalSugarContent { get; set; } // per serving
        
        // Nutritional Enhancements
        public bool? IsVitaminEnriched { get; set; }
        public List<string> AddedVitamins { get; set; } = new(); // Vitamin A, B12, D, etc.
        public bool? IsProteinEnriched { get; set; }
        public string? ProteinContent { get; set; } // per serving
        public bool? IsFiberEnriched { get; set; }
        public string? FiberContent { get; set; } // per serving
        public bool? IsCalciumFortified { get; set; }
        public bool? IsIronFortified { get; set; }
        public List<string> AddedMinerals { get; set; } = new();
        public bool? ContainsProbiotics { get; set; }
        public bool? ContainsOmega3 { get; set; }
        
        // Dietary Preferences
        public bool? IsVegan { get; set; }
        public bool? IsVegetarian { get; set; }
        public bool? IsPlantBased { get; set; }
        public bool? IsPaleo { get; set; }
        public bool? IsKeto { get; set; }
        public bool? IsLowCarb { get; set; }
        public bool? IsLowFat { get; set; }
        public bool? IsLowSodium { get; set; }
        public bool? IsHighProtein { get; set; }
        public bool? IsWholeFoods { get; set; }
        
        // Production & Quality
        public bool? IsOrganic { get; set; }
        public string? OrganicCertification { get; set; } // USDA Organic, EU Organic, etc.
        public bool? IsNonGMO { get; set; }
        public string? NonGMOCertification { get; set; } // Non-GMO Project Verified, etc.
        public bool? IsFairTrade { get; set; }
        public bool? IsLocallySourced { get; set; }
        public bool? IsGrassFed { get; set; }
        public bool? IsFreeRange { get; set; }
        public bool? IsCageFree { get; set; }
        public bool? IsWildCaught { get; set; }
        public bool? IsSustainablySourced { get; set; }
        
        // Special Dietary Needs
        public bool? IsBabyFood { get; set; }
        public string? AgeRecommendation { get; set; } // 6+ months, toddler, etc.
        public bool? IsSeniorFriendly { get; set; }
        public bool? IsHospitalGrade { get; set; }
        public bool? IsMedicalFood { get; set; }
        
        // Additional Attributes
        public bool? IsNoPreservatives { get; set; }
        public bool? IsNoArtificialColors { get; set; }
        public bool? IsNoArtificialFlavors { get; set; }
        public bool? IsAllNatural { get; set; }
        public bool? IsMinimallyProcessed { get; set; }
        public bool? IsRawFood { get; set; }
        public bool? IsFrozen { get; set; }
        public bool? IsReadyToEat { get; set; }
        public bool? RequiresCooking { get; set; }
        public bool? IsMicrowaveable { get; set; }
        
        // Package Text Analysis
        public List<string> PackageTextClaims { get; set; } = new(); // all text claims found on package
        public List<string> NutritionalClaims { get; set; } = new(); // "High in...", "Source of...", etc.
        public List<string> HealthClaims { get; set; } = new(); // "Heart healthy", "Immune support", etc.
        public string? CaloriesPerServing { get; set; }
        public string? ServingRecommendation { get; set; }
    }
}