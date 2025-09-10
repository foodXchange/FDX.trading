using Azure.Storage.Blobs.Models;

namespace FoodX.EmailService.Services;

public interface IBlobStorageService
{
    Task<string> UploadAttachmentAsync(byte[] content, string fileName, string contentType);
    Task<byte[]> DownloadAttachmentAsync(string blobUrl);
    Task<bool> DeleteAttachmentAsync(string blobUrl);
    Task<Stream> GetAttachmentStreamAsync(string blobUrl);
    Task<string> GetAttachmentDownloadUrlAsync(string blobUrl, TimeSpan expiry);
    Task<bool> AttachmentExistsAsync(string blobUrl);
}