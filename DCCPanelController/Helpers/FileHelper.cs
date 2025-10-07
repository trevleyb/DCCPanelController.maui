using System.Diagnostics;
using System.Text;
using CommunityToolkit.Maui.Storage;

namespace DCCPanelController.Helpers;

public static class FileHelper {

    public static async Task<IResult<string?>> SaveFileAsync(string title, string content, string filename, CancellationToken ct = default) {
        if (string.IsNullOrWhiteSpace(filename)) filename = "data.json";
        if (!Path.HasExtension(filename)) filename += ".json";

        // put content into a stream
        var bytes = Encoding.UTF8.GetBytes(content);
        using var mem = new MemoryStream(bytes);

        var res = await FileSaver.Default.SaveAsync(filename, mem, ct);
        return res.IsSuccessful ? Result<string?>.Ok(res.FilePath) : Result<string?>.Fail(res.Exception, "Unable to save file.");
    }
    
    public static async Task<IResult<string?>> LoadFileAsync(string title, string filetype = ".json", CancellationToken ct = default) {
        if (string.IsNullOrWhiteSpace(filetype)) filetype = "json";
        var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> {
            
            // iOS / Mac Catalyst: Uniform Type Identifiers (UTType)
            { DevicePlatform.iOS, [$"public.{filetype}", "public.text"] },
            { DevicePlatform.MacCatalyst, [$"public.{filetype}", "public.text"] },

            // Android: MIME types
            { DevicePlatform.Android, [$"application/{filetype}", $"text/{filetype}", "text/plain"] },

            // Windows: extensions
            { DevicePlatform.WinUI, [$".{filetype}", ".txt"] }
        });

        if (string.IsNullOrEmpty(title)) title = "Select a Panel file";
        var result = await FilePicker.PickAsync(new PickOptions {
            PickerTitle = title,
            FileTypes   = fileTypes,
        });

        if (result is null) return Result<string?>.Fail("Load Canceled by User.");

        await using var stream = await result.OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen:false);
        var data = await reader.ReadToEndAsync(ct);
        return Result<string?>.Ok(data);
    }
}