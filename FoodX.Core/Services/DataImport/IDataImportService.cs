namespace FoodX.Core.Services.DataImport;

public interface IDataImportService
{
    Task<ImportResult> ImportBuyersAsync(string jsonFilePath);
    Task<ImportResult> ImportSuppliersAsync(string jsonFilePath);
    Task<ImportResult> ImportExhibitorsAsync(string jsonFilePath);
}

public class ImportResult
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int ImportedRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public DateTime ImportDate { get; set; } = DateTime.UtcNow;
}