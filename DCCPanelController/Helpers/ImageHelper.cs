using System.Diagnostics;
using DCCPanelController.Helpers.Logging;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Helpers;

public static class ImageHelper {
    public static async Task<string> ImageToBase64Async(FileResult result) {
        await using var stream = await result.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();
        var base64Image = Convert.ToBase64String(imageBytes);
        return base64Image;
    }

    public static string ImageToBase64(FileResult result) => ImageToBase64Async(result).Result;

    public static ImageSource? ImageFromBase64(string base64String) {
        try {
            var imageBytes = Convert.FromBase64String(base64String);
            MemoryStream imageDecodeStream = new(imageBytes);
            return ImageSource.FromStream(() => imageDecodeStream) ?? null;
        } catch (Exception ex) {
            LogHelper.Logger.LogWarning($"Unable to convert Image from Base64: {ex.Message}");
            return null;
        }
    }
}