using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsView : ContentView {

    public ButtonActionsView(ButtonActions actions, double? width,  bool isButtonContext = true) {
        InitializeComponent();
        ButtonActions = actions;
        IsButtonContext = isButtonContext;
        BindingContext = this;
        if (width is not null) this.WidthRequest = (double)width;
        UpdateLabels();
    }

    public ObservableCollection<ButtonAction> ButtonActions { get; init; }
    private bool IsButtonContext { get; }
    public bool IsRefreshing { get; set; }

    public ButtonAction? SelectedButtonAction { get; set; }
    public ButtonAction? EditingButtonAction { get; set; }
    
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
    private void DeleteButtonAction() {
        //ButtonActions.Add(new ButtonAction());
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