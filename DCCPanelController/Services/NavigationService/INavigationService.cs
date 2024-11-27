using DCCPanelController.Model;

namespace DCCPanelController.Services.NavigationService;

public interface INavigationService {
    Task<Turnout?> NavigateToEditTurnoutAsync(Turnout turnout);
}
