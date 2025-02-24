using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGrid : ContentView {
    public ButtonActionsGrid(ButtonActions buttonActions, ITrackPiece trackPiece, ActionsContext context) {
        InitializeComponent();
        BindingContext = new ButtonActionsGridViewModel(buttonActions, trackPiece, context);
    }

    private void IDPicker_OnFocused(object? sender, FocusEventArgs e) {
        if (sender is Picker picker) {
            var selectedItem = picker.SelectedItem.ToString();
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
            var selectedItem = picker.SelectedItem.ToString();
            if (BindingContext is ButtonActionsGridViewModel viewModel) {
                picker.ItemsSource = viewModel.AvailableButtons;
                picker.SelectedItem = selectedItem;
                picker.SelectedIndex = string.IsNullOrEmpty(selectedItem) ? -1 : viewModel.AvailableButtons.IndexOf(selectedItem);
                viewModel.UpdateSelectableButtons();
            }
        }
    }
}