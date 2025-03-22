using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {

    private ButtonActionsGridViewModel _viewModel;
    public ButtonActionsGrid(ButtonActions buttonPanelActions, ActionsContext context, List<string> availableButtons) {
        _viewModel = new ButtonActionsGridViewModel(buttonPanelActions, context, availableButtons);
        InitializeComponent();
        BindingContext = _viewModel;
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
            }
        }
    }

    private void Cell_OnAppearing(object? sender, EventArgs e) { }

    private void IdPicker_OnSelectedIndexChanged(object? sender, EventArgs e) { }
}