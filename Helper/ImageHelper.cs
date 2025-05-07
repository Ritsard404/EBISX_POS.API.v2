using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EBISX_POS.API.Helper
{
    public static class ImageHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<string> DownloadAndSaveImageAsync(string imageUrl, string saveDirectory)
        {
            try
            {
                Debug.WriteLine($"Downloading image from: {imageUrl}");
                Debug.WriteLine($"Saving to directory: {saveDirectory}");

                // Ensure the directory exists
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                // Generate a unique filename
                string fileName = $"{Guid.NewGuid()}.png";
                string filePath = Path.Combine(saveDirectory, fileName);

                // Download the image
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                
                // Save the image
                await File.WriteAllBytesAsync(filePath, imageBytes);
                
                Debug.WriteLine($"Image saved successfully to: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading/saving image: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
