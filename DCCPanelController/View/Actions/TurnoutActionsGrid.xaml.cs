using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGrid : ContentView {
    public TurnoutActionsGrid(TurnoutActions actions, ActionsContext context, List<string> availableTurnouts, Action? changedAction) {
        var viewModel = new TurnoutActionsGridViewModel(actions, context, availableTurnouts, changedAction);
        viewModel.ActionsGridListView = ListView;
        BindingContext = viewModel;
        Resources["Vm"] = viewModel;
        InitializeComponent();
    }
}