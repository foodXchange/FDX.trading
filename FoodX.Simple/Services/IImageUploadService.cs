using Microsoft.AspNetCore.Components.Forms;

namespace FoodX.Simple.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(IBrowserFile file, string folder = "briefs");
        bool ValidateImageUrl(string url);
        Task<bool> IsImageUrlAccessibleAsync(string url);
        string GetImagePathUrl(string fileName);
        void DeleteImage(string filePath);
    }
}