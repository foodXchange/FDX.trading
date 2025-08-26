using System.Text.Json.Serialization;

namespace FoodX.Core.Services.DataImport.Models;

public class BuyerImportData
{
    [JsonPropertyName("metadata")]
    public ImportMetadata? Metadata { get; set; }

    [JsonPropertyName("all_buyers")]
    public List<BuyerImportModel> AllBuyers { get; set; } = new List<BuyerImportModel>();
}

public class BuyerImportModel
{
    [JsonPropertyName("company")]
    public string Company { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("categories")]
    public string? Categories { get; set; }

    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("stores")]
    public string? Stores { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("markets")]
    public string? Markets { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("procurement_email")]
    public string? ProcurementEmail { get; set; }

    [JsonPropertyName("procurement_phone")]
    public string? ProcurementPhone { get; set; }

    [JsonPropertyName("procurement_manager")]
    public string? ProcurementManager { get; set; }

    [JsonPropertyName("certifications_required")]
    public string? CertificationsRequired { get; set; }

    [JsonPropertyName("payment_terms")]
    public string? PaymentTerms { get; set; }

    [JsonPropertyName("general_emails")]
    public string? GeneralEmails { get; set; }

    [JsonPropertyName("general_phones")]
    public string? GeneralPhones { get; set; }
}

public class SupplierImportData
{
    [JsonPropertyName("metadata")]
    public ImportMetadata? Metadata { get; set; }

    [JsonPropertyName("suppliers")]
    public List<SupplierImportModel> Suppliers { get; set; } = new List<SupplierImportModel>();
}

public class SupplierImportModel
{
    [JsonPropertyName("Supplier Name")]
    public string? SupplierName { get; set; }

    [JsonPropertyName("Company Name")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("Company website")]
    public string? CompanyWebsite { get; set; }

    [JsonPropertyName("Supplier's Description & Products")]
    public string? Description { get; set; }

    [JsonPropertyName("Product Category & family (Txt)")]
    public string? ProductCategory { get; set; }

    [JsonPropertyName("Address")]
    public string? Address { get; set; }

    [JsonPropertyName("Company address")]
    public string? CompanyAddress { get; set; }

    [JsonPropertyName("Company Email")]
    public string? CompanyEmail { get; set; }

    [JsonPropertyName("Phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("Products")]
    public string? Products { get; set; }

    [JsonPropertyName("Country")]
    public string? Country { get; set; }

    [JsonPropertyName("Terms of Payment")]
    public string? TermsOfPayment { get; set; }

    [JsonPropertyName("Incoterms")]
    public string? Incoterms { get; set; }
}

public class ExhibitorImportModel
{
    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("profile_url")]
    public string? ProfileUrl { get; set; }

    [JsonPropertyName("exhibition")]
    public string Exhibition { get; set; } = string.Empty;

    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("products")]
    public string? Products { get; set; }

    [JsonPropertyName("contact")]
    public string? Contact { get; set; }
}

public class ImportMetadata
{
    [JsonPropertyName("total_buyers")]
    public int? TotalBuyers { get; set; }

    [JsonPropertyName("total_suppliers")]
    public int? TotalSuppliers { get; set; }

    [JsonPropertyName("export_date")]
    public string? ExportDate { get; set; }
}