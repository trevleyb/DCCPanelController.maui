#if IOS || MACCATALYST
using Foundation;
#endif

namespace DCCPanelController.View;

public partial class HelpPage : ContentPage {
    public HelpPage() {
        InitializeComponent();
        LoadHelpContent();
    }

    private async void LoadHelpContent() {
        try {
            var htmlContent = await GetHtmlPath("index.html");
            if (htmlContent is null) throw new ApplicationException("Failed to load Help");
            WebView.Source = htmlContent;
        } catch (Exception ex) {
            await DisplayAlert("Error", $"Failed to load help content: {ex.Message}", "OK");
        }
    }

    private async Task<string?> GetHtmlPath(string filename) {
#if ANDROID
    return $"file:///android_asset/Help/{filename}";
#elif IOS || MACCATALYST
        var path = Path.Combine(NSBundle.MainBundle.BundlePath, "Help", filename);
        return new NSUrl(path, false).AbsoluteString;
#elif WINDOWS
    return $"ms-appx-web:///Help/{filename}";
#else
    throw new NotSupportedException("Platform not supported");
#endif
    }
}