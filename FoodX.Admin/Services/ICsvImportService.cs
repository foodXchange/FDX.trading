using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FoodX.Admin.Services
{
    public enum ImportDataType
    {
        Products,
        Customers,
        Orders,
        Inventory
    }
    public interface ICsvImportService
    {
        Task<ImportSummary> ImportProductsFromCsvAsync(Stream csvStream, int supplierId);
        Task<ImportSummary> ImportProductsAsync(Stream csvStream, string fileName, string userId, int? supplierId, ImportValidationSettings settings);
        Task<List<ImportHistoryItem>> GetImportHistoryAsync(int supplierId);
        Task<byte[]> GenerateTemplateAsync(string importType);
    }

    public class ImportResult
    {
        public int RowNumber { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class ImportSummary
    {
        public int TotalRows { get; set; }
        public int TotalRecords { get; set; } // Alias for TotalRows
        public int SuccessfulImports { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedImports { get; set; }
        public int FailedRecords { get; set; }
        public int WarningRecords { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<ImportResult> Results { get; set; } = new List<ImportResult>(); // For import results
        public DateTime ImportDate { get; set; }
        public string ImportedBy { get; set; }
        public string Filename { get; set; }
        public string ImportType { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ImportHistoryItem
    {
        public int Id { get; set; }
        public DateTime ImportDate { get; set; }
        public string FileName { get; set; }
        public string Filename { get; set; } // Alias for FileName
        public string ImportType { get; set; }
        public int TotalRows { get; set; }
        public int TotalRecords { get; set; } // Alias for TotalRows
        public int SuccessfulImports { get; set; }
        public int SuccessfulRecords { get; set; } // Alias for SuccessfulImports
        public int FailedImports { get; set; }
        public int FailedRecords { get; set; }
        public int WarningRecords { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public string ImportedBy { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public int SupplierId { get; set; }
    }

    public class ImportValidationSettings
    {
        public bool AllowDuplicates { get; set; }
        public bool ValidateReferences { get; set; }
        public bool PreviewMode { get; set; }
        public int MaxRecords { get; set; }
    }
}