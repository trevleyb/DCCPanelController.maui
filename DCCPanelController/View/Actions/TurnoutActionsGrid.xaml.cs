using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGrid : ContentView {
    public TurnoutActionsGrid(IActionEntity entity, ActionsContext context, List<string> availableTurnouts) {
        InitializeComponent();
        BindingContext = new TurnoutActionsGridViewModel(entity, context, availableTurnouts);
        Resources["Vm"] = BindingContext;
    }
}