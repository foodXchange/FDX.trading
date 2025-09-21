namespace FoodX.Simple.Application.DTOs
{
    public class ProductBriefDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? BenchmarkBrandReference { get; set; }
        public string? BenchmarkWebsiteUrl { get; set; }
        public string? PackageSize { get; set; }
        public string? StorageRequirements { get; set; }
        public string? CountryOfOrigin { get; set; }
        public bool IsKosherCertified { get; set; }
        public string? KosherOrganization { get; set; }
        public string? SpecialAttributes { get; set; }
        public string? ImagePath { get; set; }
        public string? ImageUrl { get; set; }
        public string? AdditionalNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
    }

    public class CreateProductBriefDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? BenchmarkBrandReference { get; set; }
        public string? BenchmarkWebsiteUrl { get; set; }
        public string? PackageSize { get; set; }
        public string? StorageRequirements { get; set; }
        public string? CountryOfOrigin { get; set; }
        public bool IsKosherCertified { get; set; }
        public string? KosherOrganization { get; set; }
        public string? KosherSymbol { get; set; }
        public string? SpecialAttributes { get; set; }
        public string? ImagePath { get; set; }
        public string? ImageUrl { get; set; }
        public string? AdditionalNotes { get; set; }
    }

    public class UpdateProductBriefDto : CreateProductBriefDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}