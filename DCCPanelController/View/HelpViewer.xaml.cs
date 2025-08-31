// App/Controls/HelpViewer.xaml.cs

using DCCPanelController.Services;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class HelpViewer : ContentView {
    public static readonly BindableProperty DocumentProperty = BindableProperty.Create(nameof(Document), typeof(HelpDocument), typeof(HelpViewer), propertyChanged: OnDocumentChanged);

    public HelpViewer() {
        InitializeComponent();
        Web.Navigating += OnNavigating;
    }

    public HelpDocument? Document {
        get => (HelpDocument?)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public event EventHandler<HelpLinkRequestedEventArgs>? HelpLinkRequested;

    private static void OnDocumentChanged(BindableObject bindable, object oldValue, object newValue) {
        var hv = (HelpViewer)bindable;
        if (newValue is HelpDocument doc) {
#if MACCATALYST

            // Load from file URL to grant WKWebView read access to sibling files (images/)
            if (!string.IsNullOrEmpty(doc.FilePath)) {
                var fileUrl = new Uri(doc.FilePath).AbsoluteUri;
                hv.Web.Source = new UrlWebViewSource { Url = fileUrl };
                return;
            }
#endif

            // Other platforms: inline HTML is fine
            hv.Web.Source = new HtmlWebViewSource { Html = doc.Html, BaseUrl = doc.BaseUrl };
        }
    }

    // App/Controls/HelpViewer.xaml.cs (Navigating handler)
    private async void OnNavigating(object? sender, WebNavigatingEventArgs e) {
        var url = e.Url ?? "";
        if (url.StartsWith("help://", StringComparison.OrdinalIgnoreCase)) {
            e.Cancel = true;

            // Parse help://topic/{id}[#anchor]
            var after = url.Replace("help://topic/", "", StringComparison.OrdinalIgnoreCase);
            var parts = after.Split('#', 2);
            var id = parts[0];
            var anchor = parts.Length > 1 ? parts[1] : null;

            HelpLinkRequested?.Invoke(this, new HelpLinkRequestedEventArgs(id, anchor));
            return;
        }

        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
            e.Cancel = true;
            await Launcher.OpenAsync(url);
        }
    }
}