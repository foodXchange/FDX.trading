using Microsoft.AspNetCore.Components.Forms;

namespace FoodX.Simple.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageUploadService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public ImageUploadService(
            IWebHostEnvironment environment,
            ILogger<ImageUploadService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _environment = environment;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> UploadImageAsync(IBrowserFile file, string folder = "briefs")
        {
            try
            {
                // Validate file
                if (file == null || file.Size == 0)
                {
                    throw new ArgumentException("No file provided");
                }

                if (file.Size > _maxFileSize)
                {
                    throw new ArgumentException($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");
                }

                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    throw new ArgumentException($"File type {extension} is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
                }

                // Create upload directory
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
                Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(_maxFileSize).CopyToAsync(fileStream);

                _logger.LogInformation($"Image uploaded successfully: {fileName}");

                // Return relative path for storage in database
                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                throw;
            }
        }

        public bool ValidateImageUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                var uri = new Uri(url);
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsImageUrlAccessibleAsync(string url)
        {
            if (!ValidateImageUrl(url))
                return false;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

                if (response.IsSuccessStatusCode)
                {
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    return contentType != null && contentType.StartsWith("image/");
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetImagePathUrl(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "/images/no-image.png";

            if (fileName.StartsWith("http://") || fileName.StartsWith("https://"))
                return fileName;

            return fileName.StartsWith("/") ? fileName : $"/{fileName}";
        }

        public void DeleteImage(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Image deleted: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image: {filePath}");
            }
        }
    }
}