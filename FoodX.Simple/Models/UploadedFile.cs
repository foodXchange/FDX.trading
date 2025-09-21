using System.ComponentModel.DataAnnotations;

namespace FoodX.Simple.Models
{
    public class UploadedFile
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        public string FileType { get; set; } = "CSV";

        public long FileSize { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.Now;

        public string UploadedBy { get; set; } = string.Empty;

        public int RecordsProcessed { get; set; }

        public string Status { get; set; } = "Pending";

        public string? ErrorMessage { get; set; }
    }

    public class ImportedProduct
    {
        public int Id { get; set; }
        public int UploadedFileId { get; set; }
        public UploadedFile? UploadedFile { get; set; }

        public string? ProductCode { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
        public int Quantity { get; set; }
        public string? Supplier { get; set; }
        public DateTime ImportedDate { get; set; } = DateTime.Now;
    }
}