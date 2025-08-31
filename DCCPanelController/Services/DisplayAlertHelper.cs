using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DCCPanelController.Resources.Styles;
using Font = Microsoft.Maui.Font;

namespace DCCPanelController.Services;

public static class DisplayAlertHelper {
    public static async Task<bool> DisplayOkAlertAsync(string title, string message) {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");
        return await mainPage.DisplayAlert(title, message, null, "OK");
    }

    public static async Task<bool> DisplayAlertAsync(string title, string? message, string accept, string cancel) {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");
        return await mainPage.DisplayAlert(title, message, accept, cancel);
    }

    public static async Task<bool> DisplayAlertOkCancelAsync(string title, string message) {
        return await DisplayAlertAsync(title, message, "OK", "Cancel");
    }

    public static async Task<bool> DisplayAlertYesNoAsync(string title, string message) {
        return await DisplayAlertAsync(title, message, "Yes", "No");
    }

    public static async Task DisplayToastAlert(string message, double fontSize = 14, ToastDuration duration = ToastDuration.Short) {
        var cancellationTokenSource = new CancellationTokenSource();
        var toast = Toast.Make(message, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }

    public static async Task DisplaySnackAlert(string message, double fontSize = 14, double duration = 2.5) {
        var cancellationTokenSource = new CancellationTokenSource();
        var snackbarOptions = new SnackbarOptions {
            BackgroundColor = Colors.LightGray,
            TextColor = StyleHelper.FromStyle("Primary"),
            ActionButtonTextColor = Colors.Black,
            CornerRadius = new CornerRadius(10),
            Font = Font.SystemFontOfSize(fontSize),
            ActionButtonFont = Font.SystemFontOfSize(fontSize),
            CharacterSpacing = 0.5
        };

        var durationInSeconds = TimeSpan.FromSeconds(duration);
        var snackbar = Snackbar.Make(message, null, "OK", durationInSeconds, snackbarOptions);
        await snackbar.Show(cancellationTokenSource.Token);
    }
}