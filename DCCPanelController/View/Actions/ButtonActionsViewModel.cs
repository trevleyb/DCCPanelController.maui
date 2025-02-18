using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsViewModel : BaseViewModel {

    public ButtonActionsViewModel(ButtonActions actions, bool isButtonContext = true) {
        ButtonActions = actions;
        IsButtonContext = isButtonContext;
        UpdateLabels();
    }
    
    public ObservableCollection<ButtonAction> ButtonActions { get; init; }
    
    private bool IsButtonContext { get; }
    
    [ObservableProperty]
    private ButtonAction? _editingButtonAction;

    // Dynamic Column Header Labels
    public string StateLabelActive { get; private set; } = "Active";
    public string StateLabelInactive { get; private set; } = "Inactive";

    private void UpdateLabels() {
        StateLabelActive = IsButtonContext ? "Active" : "Closed";
        StateLabelInactive = IsButtonContext ? "Inactive" : "Thrown";
        OnPropertyChanged(nameof(StateLabelActive));
        OnPropertyChanged(nameof(StateLabelInactive));
    }

    [RelayCommand]
    private void AddButtonAction() {
        ButtonActions.Add(new ButtonAction());
        OnPropertyChanged(nameof(ButtonActions));
    }

    [RelayCommand]
    private void CompleteEdit() {
        EditingButtonAction = null;
        OnPropertyChanged(nameof(EditingButtonAction));
    }

    [RelayCommand]
    private void StartEdit(ButtonAction buttonToEdit) {
        ArgumentNullException.ThrowIfNull(buttonToEdit);
        EditingButtonAction = buttonToEdit;
        OnPropertyChanged(nameof(EditingButtonAction));
    }

    [RelayCommand]
    private void RowTapped(object item) {
        // SelectionChangedEventArgs
        if (item is SelectionChangedEventArgs args) {
            if (args.CurrentSelection.Count > 0 && args.CurrentSelection[0] is ButtonAction) {
                EditingButtonAction = (EditingButtonAction == args.CurrentSelection[0]) ? null : args.CurrentSelection[0] as ButtonAction;
            }
        }
    }

    [RelayCommand]
    private void Refresh() {
        IsRefreshing = true;
        IsRefreshing = false;
    }

    [RelayCommand]
    private void RemoveButtonAction(ButtonAction selectedButton) {
        if (selectedButton != null) {
            ButtonActions.Remove(selectedButton);
            OnPropertyChanged(nameof(ButtonActions));
        }
    }
}