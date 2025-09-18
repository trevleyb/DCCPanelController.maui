using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {
    public ButtonActionsGrid(ButtonActions actions, ActionsContext context, List<string> availableButtons, Action? changedAction) {
        InitializeComponent();
        
        var viewModel = new ButtonActionsGridViewModel(actions, context, availableButtons, changedAction);
        viewModel.ActionsGridListView = ListView;
        BindingContext = viewModel;
        Resources["Vm"] = viewModel;
    }
}