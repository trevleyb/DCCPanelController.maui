using CommunityToolkit.Maui.Views;
using DCCPanelController.Model;
using DCCPanelController.Services.NavigationService;
using DCCPanelController.View;

public class NavigationService(IServiceProvider serviceProvider) : INavigationService {
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<Turnout?> NavigateToEditTurnoutAsync(Turnout turnout)
    {
        var page = Application.Current.MainPage;
        if (page == null)
            throw new InvalidOperationException("MainPage is not set.");

        var popup = new TurnoutsEditView(turnout);
        if (await page.ShowPopupAsync(popup) is Turnout result) {
            return result;
        }

        return null;
    }
}