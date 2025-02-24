using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGrid : ContentView {
    public TurnoutActionsGrid(TurnoutActions turnoutActions, ActionsContext context) {
        InitializeComponent();
        BindingContext = new TurnoutActionsGridViewModel(turnoutActions, context);
    }

    private void IDPicker_OnFocused(object? sender, FocusEventArgs e) {
        if (sender is Picker picker) {
            var selectedItem = picker.SelectedItem.ToString();
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
            var selectedItem = picker.SelectedItem.ToString();
            if (BindingContext is TurnoutActionsGridViewModel viewModel) {
                picker.ItemsSource = viewModel.AvailableTurnouts;
                picker.SelectedItem = selectedItem;
                picker.SelectedIndex = string.IsNullOrEmpty(selectedItem) ? -1 : viewModel.AvailableTurnouts.IndexOf(selectedItem);
                viewModel.UpdateSelectableTurnouts();
            }
        }
    }
}