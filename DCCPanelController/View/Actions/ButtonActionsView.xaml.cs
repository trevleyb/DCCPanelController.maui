using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsView : ContentView {
   
    public ObservableCollection<ButtonAction> ButtonActions { get; init; }
    private bool IsButtonContext { get; init; }

    // Dynamic Column Header Labels
    public string StateLabelActive { get; private set; } = "Active";
    public string StateLabelInactive { get; private set; } = "Inactive";

    public ButtonActionsView(ButtonActions actions, bool isButtonContext = true) {
        InitializeComponent();
        ButtonActions = actions;
        IsButtonContext = isButtonContext;
        BindingContext = this;
        UpdateLabels();
    }

    private void UpdateLabels() {
        StateLabelActive = IsButtonContext ? "Active" : "Closed";
        StateLabelInactive = IsButtonContext ? "Inactive" : "Thrown";
        OnPropertyChanged(nameof(StateLabelActive));
        OnPropertyChanged(nameof(StateLabelInactive));
    }

    [RelayCommand]
    private void AddButtonAction() {
        ButtonActions.Add(new ButtonAction { });
    }

    [RelayCommand]
    private void EditButtonAction(ButtonAction selectedButton) {
        if (selectedButton != null) {
            // Edit logic here
        }
    }

    [RelayCommand]
    private void RemoveButtonAction(ButtonAction selectedButton) {
        if (selectedButton != null) {
            ButtonActions.Remove(selectedButton);
        }
    }
}