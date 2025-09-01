using System.Text;
using CommunityToolkit.Maui.Storage;

namespace DCCPanelController.Helpers;

public static class FileHelper {

    public static async Task<string?> ShareFileAsync(string? text, string? content, string fileName, Microsoft.Maui.Controls.View? parent = null) {
        try {
            var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            using (var fs = File.Open(tempPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                if (stream.CanSeek) stream.Position = 0;
                await stream.CopyToAsync(fs);
            }

            var shareTitle = text ?? "Export File"; 
            var shareFile = new ShareFile(tempPath);
            var shareRequest = new ShareFileRequest(title: shareTitle, file: shareFile);
            if (parent is not null) shareRequest.PresentationSourceBounds = parent.Bounds;
            await Share.RequestAsync(shareRequest);
            return tempPath;
        } catch (Exception ex) {
            Console.WriteLine($"Error saving file: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    ///     Prompts the user to choose a location to save the file and writes the specified content to that location.
    /// </summary>
    public static async Task<string?> SaveFileAsync(string? text, string? content, string fileName) {
        try {
            // Convert the content into a MemoryStream
            if (content is not null) {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                stream.Position = 0;
                var fileResult = await FileSaver.SaveAsync(fileName, stream, CancellationToken.None);
                return fileResult.FilePath;
            }
            return null;
        } catch (Exception ex) {
            Console.WriteLine($"Error saving file: {ex.Message}");
            return null;
        }
    }

    public static async Task<string?> LoadFileAsync(string? text) {
        try {
            var fileResult = await FilePicker.PickAsync(new PickOptions {
                PickerTitle = text ?? "Select a File to Upload", 
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> {
                    { DevicePlatform.iOS, new[] { "public.json" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.json" } },
                    { DevicePlatform.Android, new[] { "application/json" } },
                    { DevicePlatform.WinUI, new[] { ".json" } },
                })

            });
            if (fileResult == null) return null;

            // Read and return the file's content as a string
            await using var fileStream = await fileResult.OpenReadAsync();
            using var reader = new StreamReader(fileStream);
            return await reader.ReadToEndAsync();
        } catch (Exception ex) {
            Console.WriteLine($"Error opening file: {ex.Message}");
            return null;
        }
    }
}