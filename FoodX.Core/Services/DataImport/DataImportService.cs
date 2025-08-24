using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FoodX.Core.Data;
using FoodX.Core.Models.Entities;
using FoodX.Core.Services.DataImport.Models;

namespace FoodX.Core.Services.DataImport;

public class DataImportService : IDataImportService
{
    private readonly ILogger<DataImportService> _logger;
    private readonly IDbContextFactory<FoodXCoreContext> _contextFactory;

    public DataImportService(
        ILogger<DataImportService> logger,
        IDbContextFactory<FoodXCoreContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public async Task<ImportResult> ImportBuyersAsync(string jsonFilePath)
    {
        var result = new ImportResult();
        
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                result.Errors.Add($"File not found: {jsonFilePath}");
                return result;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var buyerData = JsonSerializer.Deserialize<BuyerImportData>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (buyerData?.AllBuyers == null)
            {
                result.Errors.Add("Invalid JSON structure");
                return result;
            }

            result.TotalRecords = buyerData.AllBuyers.Count;
            
            using var context = await _contextFactory.CreateDbContextAsync();
            
            foreach (var buyerJson in buyerData.AllBuyers)
            {
                try
                {
                    var existingBuyer = await context.Set<Buyer>()
                        .FirstOrDefaultAsync(b => b.Company == buyerJson.Company);

                    if (existingBuyer != null)
                    {
                        // Update existing buyer
                        UpdateBuyerFromJson(existingBuyer, buyerJson);
                        existingBuyer.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Create new buyer
                        var newBuyer = CreateBuyerFromJson(buyerJson);
                        context.Set<Buyer>().Add(newBuyer);
                    }

                    result.ImportedRecords++;
                }
                catch (Exception ex)
                {
                    result.FailedRecords++;
                    result.Errors.Add($"Failed to import buyer {buyerJson.Company}: {ex.Message}");
                    _logger.LogError(ex, "Error importing buyer {Company}", buyerJson.Company);
                }
            }

            await context.SaveChangesAsync();
            result.Success = result.ImportedRecords > 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Import failed: {ex.Message}");
            _logger.LogError(ex, "Error during buyer import");
        }

        return result;
    }

    public async Task<ImportResult> ImportSuppliersAsync(string jsonFilePath)
    {
        var result = new ImportResult();
        
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                result.Errors.Add($"File not found: {jsonFilePath}");
                return result;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var supplierData = JsonSerializer.Deserialize<SupplierImportData>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (supplierData?.Suppliers == null)
            {
                result.Errors.Add("Invalid JSON structure");
                return result;
            }

            result.TotalRecords = supplierData.Suppliers.Count;
            
            using var context = await _contextFactory.CreateDbContextAsync();
            
            foreach (var supplierJson in supplierData.Suppliers)
            {
                try
                {
                    var supplierName = supplierJson.SupplierName ?? supplierJson.CompanyName;
                    if (string.IsNullOrEmpty(supplierName)) continue;

                    var existingSupplier = await context.Set<Supplier>()
                        .FirstOrDefaultAsync(s => s.SupplierName == supplierName);

                    if (existingSupplier != null)
                    {
                        // Update existing supplier
                        UpdateSupplierFromJson(existingSupplier, supplierJson);
                        existingSupplier.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Create new supplier
                        var newSupplier = CreateSupplierFromJson(supplierJson);
                        context.Set<Supplier>().Add(newSupplier);
                    }

                    result.ImportedRecords++;
                }
                catch (Exception ex)
                {
                    result.FailedRecords++;
                    result.Errors.Add($"Failed to import supplier: {ex.Message}");
                    _logger.LogError(ex, "Error importing supplier");
                }
            }

            await context.SaveChangesAsync();
            result.Success = result.ImportedRecords > 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Import failed: {ex.Message}");
            _logger.LogError(ex, "Error during supplier import");
        }

        return result;
    }

    public async Task<ImportResult> ImportExhibitorsAsync(string jsonFilePath)
    {
        var result = new ImportResult();
        
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                result.Errors.Add($"File not found: {jsonFilePath}");
                return result;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var exhibitorData = JsonSerializer.Deserialize<List<ExhibitorImportModel>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (exhibitorData == null)
            {
                result.Errors.Add("Invalid JSON structure");
                return result;
            }

            result.TotalRecords = exhibitorData.Count;
            
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Group by exhibition
            var exhibitionGroups = exhibitorData.GroupBy(e => e.Exhibition);
            
            foreach (var group in exhibitionGroups)
            {
                var exhibitionName = group.Key;
                if (string.IsNullOrEmpty(exhibitionName)) continue;

                // Get or create exhibition
                var exhibition = await context.Set<Exhibition>()
                    .FirstOrDefaultAsync(e => e.Name == exhibitionName);
                    
                if (exhibition == null)
                {
                    exhibition = new Exhibition
                    {
                        Name = exhibitionName,
                        SourceUrl = group.First().SourceUrl,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Set<Exhibition>().Add(exhibition);
                    await context.SaveChangesAsync();
                }

                foreach (var exhibitorJson in group)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(exhibitorJson.CompanyName)) continue;

                        var existingExhibitor = await context.Set<Exhibitor>()
                            .FirstOrDefaultAsync(e => e.CompanyName == exhibitorJson.CompanyName 
                                && e.ExhibitionId == exhibition.Id);

                        if (existingExhibitor == null)
                        {
                            var newExhibitor = new Exhibitor
                            {
                                CompanyName = exhibitorJson.CompanyName,
                                ProfileUrl = exhibitorJson.ProfileUrl,
                                Country = exhibitorJson.Country,
                                Products = exhibitorJson.Products,
                                Contact = exhibitorJson.Contact,
                                ExhibitionId = exhibition.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            context.Set<Exhibitor>().Add(newExhibitor);
                            result.ImportedRecords++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedRecords++;
                        result.Errors.Add($"Failed to import exhibitor {exhibitorJson.CompanyName}: {ex.Message}");
                        _logger.LogError(ex, "Error importing exhibitor");
                    }
                }
            }

            await context.SaveChangesAsync();
            result.Success = result.ImportedRecords > 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Import failed: {ex.Message}");
            _logger.LogError(ex, "Error during exhibitor import");
        }

        return result;
    }

    private Buyer CreateBuyerFromJson(BuyerImportModel json)
    {
        return new Buyer
        {
            Company = json.Company,
            Type = json.Type,
            Website = json.Website,
            Categories = json.Categories,
            Size = json.Size,
            Stores = json.Stores,
            Region = json.Region,
            Markets = json.Markets,
            Domain = json.Domain,
            ProcurementEmail = json.ProcurementEmail,
            ProcurementPhone = json.ProcurementPhone,
            ProcurementManager = json.ProcurementManager,
            CertificationsRequired = json.CertificationsRequired,
            PaymentTerms = json.PaymentTerms,
            GeneralEmail = json.GeneralEmails,
            GeneralPhone = json.GeneralPhones,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void UpdateBuyerFromJson(Buyer buyer, BuyerImportModel json)
    {
        buyer.Type = json.Type ?? buyer.Type;
        buyer.Website = json.Website ?? buyer.Website;
        buyer.Categories = json.Categories ?? buyer.Categories;
        buyer.Size = json.Size ?? buyer.Size;
        buyer.Stores = json.Stores ?? buyer.Stores;
        buyer.Region = json.Region ?? buyer.Region;
        buyer.Markets = json.Markets ?? buyer.Markets;
        buyer.Domain = json.Domain ?? buyer.Domain;
        buyer.ProcurementEmail = json.ProcurementEmail ?? buyer.ProcurementEmail;
        buyer.ProcurementPhone = json.ProcurementPhone ?? buyer.ProcurementPhone;
        buyer.ProcurementManager = json.ProcurementManager ?? buyer.ProcurementManager;
        buyer.CertificationsRequired = json.CertificationsRequired ?? buyer.CertificationsRequired;
        buyer.PaymentTerms = json.PaymentTerms ?? buyer.PaymentTerms;
        buyer.GeneralEmail = json.GeneralEmails ?? buyer.GeneralEmail;
        buyer.GeneralPhone = json.GeneralPhones ?? buyer.GeneralPhone;
    }

    private Supplier CreateSupplierFromJson(SupplierImportModel json)
    {
        return new Supplier
        {
            SupplierName = json.SupplierName ?? json.CompanyName ?? "",
            CompanyWebsite = json.CompanyWebsite,
            Description = json.Description,
            ProductCategory = json.ProductCategory,
            Address = json.Address ?? json.CompanyAddress,
            CompanyEmail = json.CompanyEmail,
            Phone = json.Phone,
            ProductDetails = json.Products,
            Country = json.Country,
            PaymentTerms = json.TermsOfPayment,
            Incoterms = json.Incoterms,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void UpdateSupplierFromJson(Supplier supplier, SupplierImportModel json)
    {
        supplier.CompanyWebsite = json.CompanyWebsite ?? supplier.CompanyWebsite;
        supplier.Description = json.Description ?? supplier.Description;
        supplier.ProductCategory = json.ProductCategory ?? supplier.ProductCategory;
        supplier.Address = (json.Address ?? json.CompanyAddress) ?? supplier.Address;
        supplier.CompanyEmail = json.CompanyEmail ?? supplier.CompanyEmail;
        supplier.Phone = json.Phone ?? supplier.Phone;
        supplier.ProductDetails = json.Products ?? supplier.ProductDetails;
        supplier.Country = json.Country ?? supplier.Country;
        supplier.PaymentTerms = json.TermsOfPayment ?? supplier.PaymentTerms;
        supplier.Incoterms = json.Incoterms ?? supplier.Incoterms;
    }
}