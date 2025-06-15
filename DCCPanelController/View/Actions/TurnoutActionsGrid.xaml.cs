using CommunityToolkit.Maui.Views;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGrid : ContentView {
    public TurnoutActionsGrid(TurnoutActions turnoutPanelActions, ActionsContext context, List<string> availableTurnouts) {
        InitializeComponent();
        BindingContext = new TurnoutActionsGridViewModel(turnoutPanelActions, context, availableTurnouts);
    }
}