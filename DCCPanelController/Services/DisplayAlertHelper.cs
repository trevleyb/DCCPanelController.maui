using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

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

    public static async Task<bool> DisplayAlertOkCancelAsync(string title, string message) => await DisplayAlertAsync(title, message, "OK", "Cancel");
    public static async Task<bool> DisplayAlertYesNoAsync(string title, string message) => await DisplayAlertAsync(title, message, "Yes", "No");

    public static async Task DisplayToastAlert(string message, double fontSize = 14, ToastDuration duration = ToastDuration.Short) {
        var cancellationTokenSource = new CancellationTokenSource();
        var toast = Toast.Make(message, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }
}