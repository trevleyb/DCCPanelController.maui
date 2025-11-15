using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DCCPanelController.Resources.Styles;
using Font = Microsoft.Maui.Font;

namespace DCCPanelController.Services;

public static class DisplayAlertHelper {
    public static async Task<bool> DisplayOkAlertAsync(string title, string message) {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");
        return await mainPage.DisplayAlertAsync(title, message, null, "OK");
    }

    public static async Task<bool> DisplayAlertAsync(string title, string? message, string accept, string cancel) {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");
        return await mainPage.DisplayAlertAsync(title, message, accept, cancel);
    }

    public static async Task<bool> DisplayAlertOkCancelAsync(string title, string message) => await DisplayAlertAsync(title, message, "OK", "Cancel");

    public static async Task<bool> DisplayAlertYesNoAsync(string title, string message) => await DisplayAlertAsync(title, message, "Yes", "No");

    public static async Task DisplayToastAlert(string message, double fontSize = 14, ToastDuration duration = ToastDuration.Short) {
        var toast = Toast.Make(message, duration, fontSize);
        await toast.Show(CancellationToken.None);
    }

    public static async Task DisplaySnackAlert(string message, double fontSize = 14, double duration = 2.5) {
        var snackbarOptions = new SnackbarOptions {
            BackgroundColor = Colors.LightGray,
            TextColor = Colors.Black,
            ActionButtonFont = Font.SystemFontOfSize(fontSize),
            ActionButtonTextColor = Colors.Black,
            CornerRadius = new CornerRadius(10),
            Font = Font.SystemFontOfSize(fontSize),
            CharacterSpacing = 0.50, 
        };

        var durationInSeconds = TimeSpan.FromSeconds(duration);
        var snackbar = Snackbar.Make(message, null, "Close", durationInSeconds, snackbarOptions);
        await snackbar.Show(CancellationToken.None);
    }
}