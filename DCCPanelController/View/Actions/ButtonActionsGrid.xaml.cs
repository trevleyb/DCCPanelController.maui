using DCCPanelController.Model;
using DCCPanelController.View.PropertyPages;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {
    public ButtonActionsGrid(ButtonActions buttonActions, ActionsContext context, List<string> availableButtons) {
        InitializeComponent();
        BindingContext = new ButtonActionsGridViewModel(buttonActions, context, availableButtons);
    }

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

                // viewModel.UpdateSelectableButtons();
            }
        }
    }
}