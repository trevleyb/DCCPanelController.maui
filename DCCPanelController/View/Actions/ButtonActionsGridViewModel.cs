using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGridViewModel : ObservableObject {
    
    [ObservableProperty] private ActionsContext _actionContext;
    [ObservableProperty] private ButtonActions _buttonPanelActions;
    
    [ObservableProperty] public List<string> _availableButtons;
    [ObservableProperty] public List<string> _selectableButtons;

    public ButtonActionsGridViewModel(ButtonActions buttonPanelActions, ActionsContext context, List<string> availableButtons) {
        ActionContext = context;
        AvailableButtons = availableButtons;
        SelectableButtons = new List<string>(availableButtons);
        ButtonPanelActions = buttonPanelActions;
        UpdateSelectableButtons();
        OnPropertyChanged(nameof(IsTurnoutContext));
        OnPropertyChanged(nameof(IsButtonContext));
        OnPropertyChanged(nameof(ControlHeight));
    }

    public bool IsTurnoutContext => ActionContext == ActionsContext.Turnout;
    public bool IsButtonContext => ActionContext == ActionsContext.Button;
    public bool IsGridVisible => ButtonPanelActions.Count > 0;
    public bool IsAddButtonEnabled => SelectableButtons.Count > 0;
    public double ControlHeight => 40 + ButtonPanelActions.Count * 40;

    public string NoDataText {
        get {
            if (AvailableButtons.Count == 0) return "No Buttons have been defined. ";
            if (ButtonPanelActions.Count == 0) return "Use the + key to add a button action.";
            if (SelectableButtons.Count == 0) return "All defined buttons have been assigned.";
            return "";
        }
    }

    [RelayCommand]
    private void AddRow() {
        if (SelectableButtons.Count > 0) {
            ButtonPanelActions.Add(new ButtonAction { Id = SelectableButtons[0], WhenOn = ButtonStateEnum.On, WhenOff = ButtonStateEnum.Off, Cascade = false });
        }
        UpdateSelectableButtons();
    }

    [RelayCommand]
    private void RemoveRow(ButtonAction? buttonAction) {
        if (buttonAction is not null) {
            ButtonPanelActions.Remove(buttonAction);
            UpdateSelectableButtons();
        }
    }

    [RelayCommand]
    private void IdValueChanged(string id) {
        UpdateSelectableButtons();
        
    }

    public void UpdateSelectableButtons(string? activeButton = "") {
        for (var i = AvailableButtons.Count - 1; i >= 0; i--) {
            var button = AvailableButtons[i];

            if (ButtonPanelActions.Any(btn => btn.Id == button) && button != activeButton) {
                SelectableButtons.Remove(button);
            } else {
                // Otherwise add it to the Selectable ones as it may have been removed
                // -------------------------------------------------------------------
                if (!SelectableButtons.Contains(button)) {
                    SelectableButtons.Add(button);
                }
            }
        }

        OnPropertyChanged(nameof(ButtonPanelActions));
        OnPropertyChanged(nameof(SelectableButtons));
        OnPropertyChanged(nameof(AvailableButtons));
        OnPropertyChanged(nameof(ControlHeight));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(NoDataText));
    }
}