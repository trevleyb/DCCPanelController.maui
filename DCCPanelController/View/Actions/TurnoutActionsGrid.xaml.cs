using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGrid : ContentView {
    public TurnoutActionsGrid(IActionEntity entity, ActionsContext context, List<string> availableTurnouts) {
        InitializeComponent();
        BindingContext = new TurnoutActionsGridViewModel(entity, context, availableTurnouts);
    }

    // private void PopupSelector_OnOnPopup(object? sender, PopupSelectorEventArgs e) {
    //     if (BindingContext is TurnoutActionsGridViewModel vm) {
    //         vm.UpdateSelectableItems(e.CurrentItem?.ToString() ?? "");
    //     }
    // }
}