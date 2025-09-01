using DCCPanelController.Views.Helpers;
using Syncfusion.Maui.Toolkit.Popup;

namespace DCCPanelController.Views;

public partial class AboutPage : ContentView {

    // Add events for the popup
    public event Action? AboutPageCompleted;
    public string VersionString { get; set; }
    
    public AboutPage() {
        BindingContext = this;
        VersionString = VersionInfo.Version;
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