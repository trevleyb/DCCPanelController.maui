using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGrid : ContentView {
    public TurnoutActionsGrid(Models.DataModel.Actions<TurnoutStateEnum> turnoutActions, ActionsContext context, List<string> availableTurnouts) {
        InitializeComponent();
        BindingContext = new TurnoutActionsGridViewModel(turnoutActions, context, availableTurnouts);
    }

    private void IDPicker_OnFocused(object? sender, FocusEventArgs e) {
        if (sender is Picker picker) {
            var selectedItem = picker.SelectedItem?.ToString() ?? "";

            if (BindingContext is TurnoutActionsGridViewModel viewModel) {
                viewModel.UpdateSelectableTurnouts(selectedItem);
                picker.ItemsSource = viewModel.SelectableTurnouts;
                picker.SelectedItem = selectedItem;
                picker.SelectedIndex = string.IsNullOrEmpty(selectedItem) ? -1 : viewModel.SelectableTurnouts.IndexOf(selectedItem);
            }
        }
    }

    private void IDPicker_OnUnfocused(object? sender, FocusEventArgs e) {
        if (sender is Picker picker) {
            var selectedItem = picker.SelectedItem?.ToString() ?? "";

            if (BindingContext is TurnoutActionsGridViewModel viewModel) {
                picker.ItemsSource = viewModel.AvailableTurnouts;
                picker.SelectedItem = selectedItem;
                picker.SelectedIndex = string.IsNullOrEmpty(selectedItem) ? -1 : viewModel.AvailableTurnouts.IndexOf(selectedItem);

                //viewModel.UpdateSelectableTurnouts();
            }
        }
    }
}