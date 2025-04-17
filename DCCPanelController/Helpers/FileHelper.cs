using System.Text;
using CommunityToolkit.Maui.Storage;

namespace DCCPanelController.Helpers;

public static class FileHelper {
    /// <summary>
    ///     Prompts the user to choose a location to save the file and writes the specified content to that location.
    /// </summary>
    /// <param name="content">The content to save into the file.</param>
    /// <param name="fileName">The suggested name for the file (e.g., example.txt).</param>
    /// <returns>Returns the saved file's path or null if the user cancels or there is an error.</returns>
    public static async Task<string?> SaveFileAsync(string? text, string? content, string fileName) {
        try {
            // Convert the content into a MemoryStream
            if (content is not null) {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                var fileSaver = FileSaver.Default;
                var fileResult = await fileSaver.SaveAsync(fileName, stream, CancellationToken.None);
                return fileResult?.FilePath;
            }
            return null;
        } catch (Exception ex) {
            Console.WriteLine($"Error saving file: {ex.Message}");
            return null;
        }
    }

    public static async Task<string?> OpenFileAsync(string? text) {
        try {
            var filePicker = FilePicker.Default;
            var fileResult = await filePicker.PickAsync(new PickOptions {
                PickerTitle = text ?? "Select a Panel to Upload"
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