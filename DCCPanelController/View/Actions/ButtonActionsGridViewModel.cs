using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGridViewModel : ObservableObject {

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ControlHeight))]
    [NotifyPropertyChangedFor(nameof(IsAddButtonEnabled))]
    [NotifyPropertyChangedFor(nameof(IsGridVisible))]
    [NotifyPropertyChangedFor(nameof(NoDataText))]
    private ObservableCollection<string> _availableButtons;

    [ObservableProperty]
    private ActionsContext _actionContext;
    
    public bool IsTurnoutContext => ActionContext == ActionsContext.Turnout;
    public bool IsButtonContext => ActionContext == ActionsContext.Button;
    
    public bool IsGridVisible => ButtonActions.Count > 0;
    public bool IsAddButtonEnabled => SelectableButtons.Count > 0;
    public double ControlHeight => 40 + (ButtonActions.Count * 40);

    public ButtonActionsGridViewModel(ButtonActions buttonActions, ITrackPiece trackPiece, ActionsContext context) {
        ActionContext = context;
        AvailableButtons = FindAvailableButtons(trackPiece);
        SelectableButtons = new ObservableCollection<string>(AvailableButtons);
        ButtonActions = buttonActions;
        UpdateSelectableButtons();
        PropertyChanged += (sender, args) => { Console.WriteLine("Property Changed:" + args.PropertyName); };
        OnPropertyChanged(nameof(IsTurnoutContext));
        OnPropertyChanged(nameof(IsButtonContext));
    }

    public string NoDataText {
        get {
            if (AvailableButtons.Count == 0) return "No Buttons have been defined. Create buttons to assign actions.";
            if (ButtonActions.Count == 0) return "Use the + key to add a button action.";
            if (SelectableButtons.Count == 0) return "All defined buttons have been assigned.";
            return "";
        }
    }

    [RelayCommand]
    private void AddRow() {
        if (SelectableButtons.Count > 0) {
            ButtonActions.Add(new ButtonAction() { Id = SelectableButtons[0], WhenActiveOrClosed = ButtonStateEnum.Active, WhenInactiveOrThrown = ButtonStateEnum.Inactive, Cascade = false });
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
    /// using the current track piece, look at the parent panel collection and
    /// iterate over ALL track pieces. If any are a button, and if the name of that
    /// button is different from the current button, then add that button name
    /// to a collection of available buttons. 
    /// </summary>
    private ObservableCollection<string> FindAvailableButtons(ITrackPiece trackPiece) {
        var foundButtons = new ObservableCollection<string>();
        var thisButton = trackPiece as ITrackButton ;
        if (trackPiece is { Parent: { Tracks: { } tracks } }) {
            foreach (var track in tracks) {
                if (track is ITrackButton trackButton) {
                    if (thisButton != null && thisButton.ButtonID == trackButton.ButtonID) continue;
                    foundButtons.Add(trackButton.ButtonID);
                }
            }
        }
        return foundButtons;
    }

    public void UpdateSelectableButtons(string? activeButton = "") {
        Console.WriteLine($"Updating Selectable Buttons: {string.Join(",", SelectableButtons)}");
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
        Console.WriteLine($"Updated Selectable Buttons: {string.Join(",", SelectableButtons)}");
        OnPropertyChanged(nameof(ButtonActions));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(NoDataText));
        OnPropertyChanged(nameof(SelectableButtons));
        OnPropertyChanged(nameof(AvailableButtons));
        OnPropertyChanged(nameof(ControlHeight));
    }
}