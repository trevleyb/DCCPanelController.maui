using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {
    
    public ButtonActionsGrid(ButtonActions buttonPanelActions, ActionsContextEnum context, List<string> availableButtons) {
        InitializeComponent();
        BindingContext = new ButtonActionsGridViewModel(buttonPanelActions, context, availableButtons);
    }

    // protected override void OnSizeAllocated(double width, double height) {
    //     base.OnSizeAllocated(width, height);
    //     if (width > 0 && height > 0) { base.OnSizeAllocated(width, _viewModel.ControlHeight);}
    // }

    private void IDPicker_OnFocused(object? sender, FocusEventArgs e) {
        if (sender is Picker picker) {
            var selectedItem = picker.SelectedItem?.ToString() ?? "";
            if (BindingContext is ButtonActionsGridViewModel viewModel) {
                viewModel.UpdateSelectableButtons(selectedItem);
                picker.ItemsSource = viewModel.SelectableButtons;
                picker.SelectedItem = selectedItem;
                picker.SelectedIndex = string.IsNullOrEmpty(selectedItem) ? -1 : viewModel.SelectableButtons.IndexOf(selectedItem);
            }
        }
    }

    private void IDPicker_OnUnfocused(object? sender, FocusEventArgs e) {
        if (sender is Picker picker) {
            var selectedItem = picker.SelectedItem?.ToString() ?? "";
            if (BindingContext is ButtonActionsGridViewModel viewModel) {
                picker.ItemsSource = viewModel.AvailableButtons;
                picker.SelectedItem = selectedItem;
                picker.SelectedIndex = string.IsNullOrEmpty(selectedItem) ? -1 : viewModel.AvailableButtons.IndexOf(selectedItem);
            }
        }
    }
}