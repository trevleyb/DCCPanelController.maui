using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Popup;
using Microsoft.Maui.ApplicationModel;

namespace DCCPanelController.View;

public partial class AboutPage : ContentView {

    // Add events for the popup
    public event Action? AboutPageCompleted;
    public string VersionString { get; set; }
    public Version SystemVersion { get; set; }
    
    public AboutPage() {
        VersionString = $"Version {AppInfo.Current.VersionString}"; // e.g., "1.2.3"
        SystemVersion = AppInfo.Current.Version;       // System.Version if you prefer
        InitializeComponent();
    }
    
    public static async Task ShowAbout() {
        var tcs = new TaskCompletionSource();
        var aboutPage = new AboutPage();
        var popup = new SfPopup {
            ContentTemplate = new DataTemplate(() => aboutPage),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            BackgroundColor = Colors.White,
            OverlayMode = PopupOverlayMode.Blur,
            
            PopupStyle = new PopupStyle {
                CornerRadius = 15,
                BlurIntensity = PopupBlurIntensity.Dark,
                HasShadow = true,
            },
            AutoSizeMode = PopupAutoSizeMode.Both,
            AnimationMode = PopupAnimationMode.Fade,
            AnimationDuration = 500
        };

        aboutPage.AboutPageCompleted += () => {
            popup.Dismiss();
            tcs.SetResult();
        };

        // Handle popup closing
        popup.Closed += (_, args) => {
            if (!tcs.Task.IsCompleted) {
                tcs.SetResult();
            }
        };
        popup.Show();
    }

    private void CloseAbout(object? sender, EventArgs e) {
        AboutPageCompleted?.Invoke();
    }
}