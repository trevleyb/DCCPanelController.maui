namespace DCCPanelController.Services;

public static class DisplayAlertHelper {
    public static async Task<bool> DisplayOkAlertAsync(string title, string message) {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");
        return await mainPage.DisplayAlert(title, message, null, "OK");
    }

    public static async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel) {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");
        return await mainPage.DisplayAlert(title, message, accept, cancel);
    }
}