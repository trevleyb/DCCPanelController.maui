using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {
    public ButtonActionsGrid(IActionEntity entity, ActionsContext context,List<string> availableButtons) {
        InitializeComponent();
        BindingContext = new ButtonActionsGridViewModel(entity, context, availableButtons);
    }
    
    private void PopupSelector_OnOnPopup(object? sender, PopupSelectorEventArgs e) {
        if (BindingContext is ButtonActionsGridViewModel vm) {
            vm.UpdateSelectableItems(e.CurrentItem?.ToString() ?? "");
        }
    }

}