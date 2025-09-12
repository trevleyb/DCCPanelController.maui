using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {
    public ButtonActionsGrid(IActionEntity entity, ActionsContext context, List<string> availableButtons) {
        InitializeComponent();
        BindingContext = new ButtonActionsGridViewModel(entity, context, availableButtons);
        Resources["Vm"] = BindingContext;
    }
}