using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Platform;

public record FontValidationResult(FontFace Font, bool FileExists, bool PlatformResolved, string? Detail);

public static class FontValidationService {
    public static async Task<IReadOnlyList<FontValidationResult>> ValidateAsync(IReadOnlyCollection<FontFace> fonts, IMauiContext mauiContext) {
        var results = new List<FontValidationResult>();

        foreach (var f in fonts) {
            // 1) Package/file presence check (app bundle)
            var fileExists = await FileExistsInAppPackageAsync(f.Filename);
            if (!fileExists) {
                results.Add(new FontValidationResult(f, false, false, "Font file not found in app package"));
                continue;
            }

            // 2) Platform resolution check via handler
            var label = new Label {
                Text = "Aa",
                FontFamily = f.Alias,
                FontSize = 12
            };

            try {
                var handler = label.ToHandler(mauiContext);
                #if IOS || MACCATALYST
                var uiLabel = (UIKit.UILabel)handler.PlatformView!;
                var resolvedName = uiLabel.Font?.Name; // e.g., your alias maps to a UIFont
                bool ok = !string.IsNullOrEmpty(resolvedName);
                results.Add(new FontValidationResult(f, true, ok, ok ? $"UIFont: {resolvedName}" : "UIFont resolution failed"));
                #elif ANDROID
                var tv = (Android.Widget.TextView)handler.PlatformView!;
                var tf = tv.Typeface;
                // Typeface.Create(alias, ...) will return a Typeface; to detect fallback, compare family name on Paint
                using var paint = new Android.Graphics.Paint();
                paint.SetTypeface(tf);
                string? fam = tf?.ToString(); // Often includes family name; not 100% consistent across OEMs
                bool ok = tf is not null && !string.IsNullOrWhiteSpace(fam);
                results.Add(new FontValidationResult(f, true, ok, ok ? $"Typeface: {fam}" : "Typeface resolution failed"));
                #elif WINDOWS
                var tb = (Microsoft.UI.Xaml.Controls.TextBlock)handler.PlatformView!;
                var src = tb.FontFamily?.Source;
                // When alias is valid, Source should be your alias (or a resolved family).
                bool ok = !string.IsNullOrWhiteSpace(src);
                results.Add(new FontValidationResult(f, true, ok, ok ? $"FontFamily.Source: {src}" : "FontFamily resolution failed"));
                #else
                results.Add(new FontValidationResult(f, true, false, "Unsupported platform for validation"));
                #endif
            } catch (Exception ex) {
                results.Add(new FontValidationResult(f, true, false, $"Exception during handler/resolve: {ex.Message}"));
            }
        }

        return results;
    }

    private static async Task<bool> FileExistsInAppPackageAsync(string filename) {
        // MAUI flattens Resources/Fonts into the app package.
        // Try open; if it throws, it isn’t present with that name.
        try {
            using var _ = await FileSystem.OpenAppPackageFileAsync(filename);
            return true;
        } catch (FileNotFoundException) {
            // Try with "Resources/Fonts/" prefix in case the filename was given with path elsewhere.
            try {
                using var __ = await FileSystem.OpenAppPackageFileAsync(Path.Combine("Resources", "Fonts", filename));
                return true;
            } catch {
                return false;
            }
        } catch {
            return false;
        }
    }
}