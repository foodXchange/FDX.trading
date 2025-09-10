using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System.Security.Cryptography;
using System.Text;

namespace FoodX.EmailService.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly string _containerName = "email-attachments";

    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _logger = logger;

        try
        {
            // Try to get connection string from Key Vault or configuration
            var connectionString = configuration["AzureStorage:ConnectionString"] 
                ?? configuration["Azure--Storage--ConnectionString"]
                ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            if (!string.IsNullOrEmpty(connectionString))
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                _logger.LogInformation("Blob Storage initialized with connection string");
            }
            else
            {
                // Fallback to storage account with managed identity
                var storageAccountName = configuration["AzureStorage:AccountName"] 
                    ?? configuration["Azure--Storage--AccountName"]
                    ?? "fdxstoragepoland"; // Default storage account

                var storageUri = new Uri($"https://{storageAccountName}.blob.core.windows.net");
                _blobServiceClient = new BlobServiceClient(storageUri, new DefaultAzureCredential());
                _logger.LogInformation($"Blob Storage initialized with managed identity for account: {storageAccountName}");
            }

            _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Blob Storage service");
            throw;
        }
    }

    public async Task<string> UploadAttachmentAsync(byte[] content, string fileName, string contentType)
    {
        try
        {
            // Ensure container exists
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Generate unique blob name with timestamp and hash
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var hash = GenerateFileHash(content);
            var sanitizedFileName = SanitizeFileName(fileName);
            var blobName = $"{timestamp}-{hash}-{sanitizedFileName}";

            var blobClient = _containerClient.GetBlobClient(blobName);

            // Set content type and other metadata
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                },
                Metadata = new Dictionary<string, string>
                {
                    ["OriginalFileName"] = fileName,
                    ["UploadedAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["FileSize"] = content.Length.ToString()
                }
            };

            using (var stream = new MemoryStream(content))
            {
                await blobClient.UploadAsync(stream, uploadOptions);
            }

            _logger.LogInformation($"Successfully uploaded attachment: {fileName} as {blobName}");
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to upload attachment: {fileName}");
            throw;
        }
    }

    public async Task<byte[]> DownloadAttachmentAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            var response = await blobClient.DownloadContentAsync();
            
            _logger.LogInformation($"Successfully downloaded attachment from: {blobUrl}");
            return response.Value.Content.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to download attachment from: {blobUrl}");
            throw;
        }
    }

    public async Task<Stream> GetAttachmentStreamAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            var response = await blobClient.OpenReadAsync();
            
            _logger.LogDebug($"Opened stream for attachment: {blobUrl}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to open stream for attachment: {blobUrl}");
            throw;
        }
    }

    public async Task<string> GetAttachmentDownloadUrlAsync(string blobUrl, TimeSpan expiry)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Attachment not found: {blobUrl}");
            }

            // Generate SAS token for secure access
            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = blobClient.Name,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
                };
                
                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                
                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                _logger.LogDebug($"Generated SAS URL for attachment: {blobUrl}");
                return sasUri.ToString();
            }
            else
            {
                // If SAS cannot be generated, return direct URL (less secure)
                _logger.LogWarning("Cannot generate SAS token, returning direct URL");
                return blobUrl;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to generate download URL for: {blobUrl}");
            throw;
        }
    }

    public async Task<bool> DeleteAttachmentAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            var response = await blobClient.DeleteIfExistsAsync();
            
            if (response.Value)
            {
                _logger.LogInformation($"Successfully deleted attachment: {blobUrl}");
            }
            else
            {
                _logger.LogWarning($"Attachment not found for deletion: {blobUrl}");
            }
            
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete attachment: {blobUrl}");
            return false;
        }
    }

    public async Task<bool> AttachmentExistsAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            var response = await blobClient.ExistsAsync();
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to check if attachment exists: {blobUrl}");
            return false;
        }
    }

    private string GenerateFileHash(byte[] content)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(content);
            return Convert.ToHexString(hashBytes)[..8]; // First 8 characters
        }
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters from file name
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        // Limit length and ensure it's not empty
        if (string.IsNullOrWhiteSpace(sanitized))
            sanitized = "attachment";
        
        return sanitized.Length > 50 ? sanitized[..50] : sanitized;
    }
}