using DCCPanelController.Helpers;
using DCCPanelController.View.Helpers;
using Syncfusion.Maui.Toolkit.Popup;

namespace DCCPanelController.View;

public partial class AboutPage : ContentView {
    public AboutPage() {
        BindingContext = this;
        VersionString = VersionInfo.Version;
        InitializeComponent();
    }

    public string VersionString { get; set; }

    // Add events for the popup
    public event Action? AboutPageCompleted;

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
            AnimationDuration = 500,
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

    private void CloseAbout(object? sender, EventArgs e) => AboutPageCompleted?.Invoke();
}