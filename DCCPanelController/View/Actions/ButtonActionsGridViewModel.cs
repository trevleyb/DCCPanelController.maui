using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGridViewModel : ObservableObject {
    [ObservableProperty] private ActionsContext _actionContext;
    [ObservableProperty] public List<string> _availableButtons;
    [ObservableProperty] private ButtonActions _buttonPanelActions;

    public ButtonActionsGridViewModel(ButtonActions buttonPanelActions, ActionsContext context, List<string> availableButtons) {
        ActionContext = context;
        AvailableButtons = availableButtons;
        ButtonPanelActions = buttonPanelActions;
        OnPropertyChanged(nameof(IsTurnoutContext));
        OnPropertyChanged(nameof(IsButtonContext));
        OnPropertyChanged(nameof(ControlHeight));
        OnPropertyChanged(nameof(IsGridVisible));

    }

    public List<string> SelectableButtons {
        get => BuildSelectableButtons();
        set => _ = value;
    }

    public bool IsTurnoutContext => ActionContext == ActionsContext.Turnout;
    public bool IsButtonContext => ActionContext == ActionsContext.Button;
    public bool IsGridVisible => ButtonPanelActions.Count > 0;
    public bool IsAddButtonEnabled => SelectableButtons.Count > 0;
    public double ControlHeight => 40 + ButtonPanelActions.Count * 40;

    public string NoDataText {
        get {
            if (AvailableButtons.Count == 0) return "No available Buttons defined. ";
            if (ButtonPanelActions.Count == 0) return "Use the + key to add a button action.";
            if (SelectableButtons.Count == 0) return "All defined buttons have been assigned.";
            return "";
        }
    }

    private void RaisePropertiesChanged() {
        OnPropertyChanged(nameof(IsTurnoutContext));
        OnPropertyChanged(nameof(IsButtonContext));
        OnPropertyChanged(nameof(ControlHeight));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(NoDataText));
    }
    
    [RelayCommand]
    private void AddRow() {
        if (SelectableButtons.Count > 0) {
            ButtonPanelActions.Add(new ButtonAction { Id = SelectableButtons[0], WhenOn = ButtonStateEnum.On, WhenOff = ButtonStateEnum.Off, Cascade = false });
        }
        RaisePropertiesChanged();
    }

    [RelayCommand]
    private void RemoveRow(ButtonAction? buttonAction) {
        if (buttonAction is not null) {
            ButtonPanelActions.Remove(buttonAction);
        }
        RaisePropertiesChanged();
    }

    [RelayCommand]
    private void IdValueChanged(string id) {
        RaisePropertiesChanged();
    }

    private List<string> BuildSelectableButtons(string? activeButton = null) {
        var selectableButtons = new List<string>(AvailableButtons);
        for (var i = AvailableButtons.Count - 1; i >= 0; i--) {
            var button = AvailableButtons[i];

            if (ButtonPanelActions.Any(btn => btn.Id == button) && button != activeButton) {
                if (selectableButtons.Contains(button)) selectableButtons.Remove(button);
            } else {
                // Otherwise add it to the Selectable ones as it may have been removed
                // -------------------------------------------------------------------
                if (!selectableButtons.Contains(button)) {
                    selectableButtons.Add(button);
                }
            }
        }
        return selectableButtons;
    }
}