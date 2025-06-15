using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities.Actions;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {
    public ButtonActionsGrid(ButtonActions buttonPanelActions, ActionsContext context, List<string> availableButtons) {
        InitializeComponent();
        BindingContext = new ButtonActionsGridViewModel(buttonPanelActions, context, availableButtons);
    }
}