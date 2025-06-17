using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DCCPanelController.View;

public partial class HelpPage : ContentPage {
    public HelpPage() {
        InitializeComponent();
        LoadHelpContent();
    }

    private async void LoadHelpContent() {
        try {
            var htmlContent = await LoadEmbeddedResource("Resources.Help.index.html");

            // Create WebView with HTML source
            var webView = new WebView {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };

            // Set the HTML content
            var htmlSource = new HtmlWebViewSource {
                Html = htmlContent,
                BaseUrl = "ms-appx-web:///Resources/Help/" // For local resource access
            };

            webView.Source = htmlSource;
            Content = webView;
        } catch (Exception ex) {
            await DisplayAlert("Error", $"Failed to load help content: {ex.Message}", "OK");
        }
    }

    private async Task<string> LoadEmbeddedResource(string resourceName) {
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Resource {resourceName} not found");

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}