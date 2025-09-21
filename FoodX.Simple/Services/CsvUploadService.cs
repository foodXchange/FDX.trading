using CsvHelper;
using CsvHelper.Configuration;
using FoodX.Simple.Data;
using FoodX.Simple.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace FoodX.Simple.Services
{
    public interface ICsvUploadService
    {
        Task<UploadedFile> ProcessCsvFileAsync(Stream fileStream, string fileName, string uploadedBy);
        Task<List<UploadedFile>> GetUploadedFilesAsync();
        Task<List<ImportedProduct>> GetImportedProductsAsync(int uploadedFileId);
    }

    public class CsvUploadService : ICsvUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CsvUploadService> _logger;

        public CsvUploadService(ApplicationDbContext context, ILogger<CsvUploadService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UploadedFile> ProcessCsvFileAsync(Stream fileStream, string fileName, string uploadedBy)
        {
            var uploadedFile = new UploadedFile
            {
                FileName = fileName,
                FileType = "CSV",
                FileSize = fileStream.Length,
                UploadedBy = uploadedBy,
                UploadedDate = DateTime.Now,
                Status = "Processing"
            };

            try
            {
                _context.UploadedFiles.Add(uploadedFile);
                await _context.SaveChangesAsync();

                var products = new List<ImportedProduct>();

                using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                }))
                {
                    await csv.ReadAsync();
                    csv.ReadHeader();

                    while (await csv.ReadAsync())
                    {
                        var product = new ImportedProduct
                        {
                            UploadedFileId = uploadedFile.Id,
                            ProductCode = GetFieldValue(csv, new[] { "ProductCode", "Code", "SKU", "Product Code" }),
                            Name = GetFieldValue(csv, new[] { "Name", "ProductName", "Product Name", "Title" }),
                            Category = GetFieldValue(csv, new[] { "Category", "Type", "Product Category" }),
                            Description = GetFieldValue(csv, new[] { "Description", "Details", "Product Description" }),
                            Price = ParseDecimal(GetFieldValue(csv, new[] { "Price", "Cost", "Unit Price", "UnitPrice" })),
                            Unit = GetFieldValue(csv, new[] { "Unit", "UOM", "Unit of Measure" }),
                            Quantity = ParseInt(GetFieldValue(csv, new[] { "Quantity", "Qty", "Stock", "StockQuantity" })),
                            Supplier = GetFieldValue(csv, new[] { "Supplier", "Vendor", "Supplier Name", "SupplierName" }),
                            ImportedDate = DateTime.Now
                        };

                        products.Add(product);
                    }
                }

                if (products.Any())
                {
                    await _context.ImportedProducts.AddRangeAsync(products);
                    await _context.SaveChangesAsync();
                }

                uploadedFile.RecordsProcessed = products.Count;
                uploadedFile.Status = "Completed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CSV file: {FileName}", fileName);
                uploadedFile.Status = "Failed";
                uploadedFile.ErrorMessage = ex.Message;
            }

            _context.Update(uploadedFile);
            await _context.SaveChangesAsync();

            return uploadedFile;
        }

        private string? GetFieldValue(CsvReader csv, string[] possibleHeaders)
        {
            foreach (var header in possibleHeaders)
            {
                try
                {
                    var value = csv.GetField<string>(header);
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
                catch
                {
                    // Header doesn't exist, try next
                }
            }
            return null;
        }

        private decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Replace("$", "").Replace(",", "").Trim();

            if (decimal.TryParse(value, out var result))
                return result;

            return 0;
        }

        private int ParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.Replace(",", "").Trim();

            if (int.TryParse(value, out var result))
                return result;

            return 0;
        }

        public async Task<List<UploadedFile>> GetUploadedFilesAsync()
        {
            return await _context.UploadedFiles
                .OrderByDescending(f => f.UploadedDate)
                .ToListAsync();
        }

        public async Task<List<ImportedProduct>> GetImportedProductsAsync(int uploadedFileId)
        {
            return await _context.ImportedProducts
                .Where(p => p.UploadedFileId == uploadedFileId)
                .ToListAsync();
        }
    }
}