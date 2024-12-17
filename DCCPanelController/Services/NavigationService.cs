using DCCPanelController.Model;
using DCCPanelController.View;
using DCCPanelController.View.PropertPages;

namespace DCCPanelController.Services.NavigationService;

public class NavigationService(IServiceProvider serviceProvider) {
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel) {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");
        return await mainPage.DisplayAlert(title, message, accept, cancel);
    }
    
    // public async Task<Panel?> NavigateToPanelEditor(Panel panel) {
    //     var mainPage = App.Current.Windows[0].Page;
    //     if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");
    //
    //     var editPage = new PanelEditorPage(panel);
    //     var tcs = new TaskCompletionSource<Panel?>();
    //     if (editPage?.ViewModel != null) {
    //         editPage.ViewModel.OnSaveCompleted += panelResult => {
    //             tcs.SetResult(panelResult);
    //             mainPage.Navigation.PopAsync();
    //         };
    //     }
    //     await mainPage.Navigation.PushAsync(editPage);
    //     return await tcs.Task;
    // }

    public static async Task NavigateToPopupWindow(ContentPage page) {
        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");

        var tcs = new TaskCompletionSource();
        ((IPropertyPage)page).CloseRequested += (sender, e) => {
            tcs.SetResult();
            mainPage.Navigation.PopModalAsync();
        };
        await mainPage.Navigation.PushModalAsync(page,true);
        await tcs.Task;
    }

    public async Task<Turnout?> NavigateToEditTurnoutAsync(Turnout? turnout) {
        if (turnout is null) return null;

        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");

        //var popup = new TurnoutsEditView(turnout);

        var editPage = new TurnoutsEditView(turnout);
        var tcs = new TaskCompletionSource<Turnout?>();
        if (editPage.ViewModel != null) {
            editPage.ViewModel.OnSaveCompleted += turnoutResult => {
                tcs.SetResult(turnoutResult);
                mainPage.Navigation.PopModalAsync();
            };
        }

        await mainPage.Navigation.PushModalAsync(editPage);
        return await tcs.Task;
    }

    private bool IsSmallDevice() {
        return DeviceInfo.DeviceType == DeviceType.Physical && DeviceInfo.Idiom == DeviceIdiom.Phone;
    }
}