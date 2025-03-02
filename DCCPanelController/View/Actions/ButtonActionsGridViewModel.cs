using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.View.PropertyPages;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGridViewModel : ObservableObject {
    [ObservableProperty]
    private ActionsContext _actionContext;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ControlHeight))]
    [NotifyPropertyChangedFor(nameof(IsAddButtonEnabled))]
    [NotifyPropertyChangedFor(nameof(IsGridVisible))]
    [NotifyPropertyChangedFor(nameof(NoDataText))]
    private ObservableCollection<string> _availableButtons;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ControlHeight))]
    [NotifyPropertyChangedFor(nameof(IsAddButtonEnabled))]
    [NotifyPropertyChangedFor(nameof(IsGridVisible))]
    [NotifyPropertyChangedFor(nameof(NoDataText))]
    private ObservableCollection<ButtonAction> _buttonActions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ControlHeight))]
    [NotifyPropertyChangedFor(nameof(IsAddButtonEnabled))]
    [NotifyPropertyChangedFor(nameof(IsGridVisible))]
    [NotifyPropertyChangedFor(nameof(NoDataText))]
    private ObservableCollection<string> _selectableButtons;

    public ButtonActionsGridViewModel(ButtonActions buttonActions, ActionsContext context, List<string> availableButtons) {
        ActionContext = context;
        AvailableButtons = availableButtons.ToObservableCollection();
        SelectableButtons = new ObservableCollection<string>(AvailableButtons);
        ButtonActions = buttonActions;
        UpdateSelectableButtons();
        OnPropertyChanged(nameof(IsTurnoutContext));
        OnPropertyChanged(nameof(IsButtonContext));
    }

    public bool IsTurnoutContext => ActionContext == ActionsContext.Turnout;
    public bool IsButtonContext => ActionContext == ActionsContext.Button;

    public bool IsGridVisible => ButtonActions.Count > 0;
    public bool IsAddButtonEnabled => SelectableButtons.Count > 0;
    public double ControlHeight => 40 + ButtonActions.Count * 40;

    public string NoDataText {
        get {
            if (AvailableButtons.Count == 0) return "No Buttons have been defined. ";
            if (ButtonActions.Count == 0) return "Use the + key to add a button action.";
            if (SelectableButtons.Count == 0) return "All defined buttons have been assigned.";
            return "";
        }
    }

    [RelayCommand]
    private void AddRow() {
        if (SelectableButtons.Count > 0) {
            ButtonActions.Add(new ButtonAction { Id = SelectableButtons[0], WhenActiveOn = ButtonStateEnum.Active, WhenInactiveOff = ButtonStateEnum.Inactive, Cascade = false });
        }

        UpdateSelectableButtons();
    }

    [RelayCommand]
    private void RemoveRow(ButtonAction action) {
        ButtonActions.Remove(action);
        UpdateSelectableButtons();
    }

    [RelayCommand]
    private void IdValueChanged(string id) {
        UpdateSelectableButtons();
    }

    /// <summary>
    ///     using the current track piece, look at the parent panel collection and
    ///     iterate over ALL track pieces. If any are a button, and if the name of that
    ///     button is different from the current button, then add that button name
    ///     to a collection of available buttons.
    /// </summary>

    // private ObservableCollection<string> FindAvailableButtons(ITrack track) {
    //     var foundButtons = new ObservableCollection<string>();
    //     var thisButton = track as ITrackButton ;
    //     if (track is { Parent: { Tracks: { } tracks } }) {
    //         foreach (var trk in tracks) {
    //             if (trk is ITrackButton trackButton) {
    //                 if (thisButton != null && thisButton.ButtonID == trackButton.ButtonID) continue;
    //                 if (string.IsNullOrWhiteSpace(trackButton.ButtonID)) continue;
    //                 foundButtons.Add(trackButton.ButtonID);
    //             }
    //         }
    //     }
    //     return foundButtons;
    // }
    public void UpdateSelectableButtons(string? activeButton = "") {
        for (var i = AvailableButtons.Count - 1; i >= 0; i--) {
            var button = AvailableButtons[i];

            // If we have already used this button, then remove it from the Selectable ones
            // ---------------------------------------------------------------------------
            var found = ButtonActions.Any(btn => btn.Id == button);

            if (ButtonActions.Any(btn => btn.Id == button) && button != activeButton) {
                SelectableButtons.Remove(button);
            } else {
                // Otherwise add it to the Selectable ones as it may have been removed
                // -------------------------------------------------------------------
                if (!SelectableButtons.Contains(button)) {
                    SelectableButtons.Add(button);
                }
            }
        }

        OnPropertyChanged(nameof(ButtonActions));
        OnPropertyChanged(nameof(SelectableButtons));
        OnPropertyChanged(nameof(AvailableButtons));
        OnPropertyChanged(nameof(TurnoutActions));
        OnPropertyChanged(nameof(ControlHeight));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(NoDataText));
    }
}