using System.Globalization;
using System.Text.RegularExpressions;
using DCCPanelController.Helpers;
using Microsoft.Maui.Controls;

namespace DCCPanelController.Models.ViewModel.Helpers;

/// <summary>
/// Converts a Base64 string (optionally data-URI formatted) or byte[] into an ImageSource.
/// Also accepts file paths or http(s) URLs for convenience.
/// </summary>
public sealed class Base64ToImageSourceConverter : IValueConverter {
    private static readonly Regex DataUriPrefix =
        new(@"^data:image\/[a-zA-Z0-9.+-]+;base64,", RegexOptions.Compiled);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is null) return Default();

        if (value is ImageSource src) return src;

        if (value is byte[] bytes && bytes.Length > 0)
            return ImageSource.FromStream(() => new MemoryStream(bytes, writable: false));

        if (value is string s) {
            return string.IsNullOrEmpty(s) ? Default() : ImageHelper.ImageFromBase64(s.Trim());
        }
        return Default();
        
        ImageSource? Default() {
            // Allow either an ImageSource or a string path (MauiImage) as the parameter
            if (parameter is ImageSource isrc) return isrc;
            if (parameter is string path && !string.IsNullOrWhiteSpace(path)) return ImageSource.FromFile(path); // assumes image.svg is a MauiImage
            return null;
        }
    }
    


    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

    private static string RemoveWhitespace(string s) => Regex.Replace(s, @"\s+", "");

    private static bool LooksLikeBase64(string s) {
        // Heuristic: Base64 is composed of A–Z, a–z, 0–9, +, / with optional = padding
        // Avoid treating short file names as base64.
        if (s.Length < 16) return false;
        return Regex.IsMatch(s, @"^[a-zA-Z0-9+/=\s]+$");
    }

    private static string PadBase64(string s) {
        var mod = s.Length % 4;
        return mod == 0 ? s : s + new string('=', 4 - mod);
    }
}