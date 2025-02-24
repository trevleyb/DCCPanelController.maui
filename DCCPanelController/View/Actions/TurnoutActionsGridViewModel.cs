using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGridViewModel : ObservableObject {

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ControlHeight))]
    [NotifyPropertyChangedFor(nameof(IsAddButtonEnabled))]
    [NotifyPropertyChangedFor(nameof(IsGridVisible))]
    [NotifyPropertyChangedFor(nameof(NoDataText))]
    private ObservableCollection<TurnoutAction> _turnoutActions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ControlHeight))]
    [NotifyPropertyChangedFor(nameof(IsAddButtonEnabled))]
    [NotifyPropertyChangedFor(nameof(IsGridVisible))]
    [NotifyPropertyChangedFor(nameof(NoDataText))]
    private ObservableCollection<string> _selectableTurnouts;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ControlHeight))]
    [NotifyPropertyChangedFor(nameof(IsAddButtonEnabled))]
    [NotifyPropertyChangedFor(nameof(IsGridVisible))]
    [NotifyPropertyChangedFor(nameof(NoDataText))]
    private ObservableCollection<string> _availableTurnouts;

    [ObservableProperty]
    private ActionsContext _actionContext;
    
    public bool IsTurnoutContext => ActionContext == ActionsContext.Turnout;
    public bool IsButtonContext => ActionContext == ActionsContext.Button;
    
    public bool IsGridVisible => TurnoutActions.Count > 0;
    public bool IsAddButtonEnabled => SelectableTurnouts.Count > 0;
    public double ControlHeight => 40 + (TurnoutActions.Count * 40);

    public TurnoutActionsGridViewModel(TurnoutActions turnoutActions, ActionsContext context) {
        ActionContext = context;
        //TODO: AvailableTurnouts = FindAvailableTurnouts(track);
        SelectableTurnouts = new ObservableCollection<string>(AvailableTurnouts);
        TurnoutActions = turnoutActions;
        UpdateSelectableTurnouts();
        OnPropertyChanged(nameof(IsTurnoutContext));
        OnPropertyChanged(nameof(IsButtonContext));
    }

    public string NoDataText {
        get {
            if (AvailableTurnouts.Count == 0) return "No Turnouts have been defined. ";
            if (TurnoutActions.Count == 0) return "Use the + key to add a turnout action.";
            if (SelectableTurnouts.Count == 0) return "All defined turnouts have been assigned.";
            return "";
        }
    }

    [RelayCommand]
    private void AddRow() {
        if (SelectableTurnouts.Count > 0) {
            TurnoutActions.Add(new TurnoutAction() { Id = SelectableTurnouts[0], WhenClosedOrActive = TurnoutStateEnum.Closed, WhenThrownOrInActive = TurnoutStateEnum.Thrown, Cascade = false });
        }
        UpdateSelectableTurnouts();
    }

    [RelayCommand]
    private void RemoveRow(TurnoutAction action) {
        TurnoutActions.Remove(action);
        UpdateSelectableTurnouts();
    }

    [RelayCommand]
    private void IdValueChanged(string id) {
        UpdateSelectableTurnouts();
    }

    // Build a collection of all available turnouts that could be selected.
    // However, if this control IS A Turnout then we can select this one so exclude it. 
    private ObservableCollection<string> FindAvailableTurnouts(ITrack track) {
        var foundTurnouts = new ObservableCollection<string>();
        var thisTurnout = track as ITrackTurnout ;
        if (track is { Parent: { Tracks: { } tracks } }) {
            foreach (var trk in tracks) {
                if (trk is ITrackTurnout trackTurnout) {
                    if (thisTurnout != null && trackTurnout.TurnoutID == thisTurnout.TurnoutID) continue;
                    if (string.IsNullOrWhiteSpace(trackTurnout.TurnoutID)) continue;
                    foundTurnouts.Add(trackTurnout.TurnoutID);
                }
            }
        }
        return foundTurnouts;
    }

    public void UpdateSelectableTurnouts(string? activeTurnout = "") {
        for (var i = AvailableTurnouts.Count - 1; i >= 0; i--) {
            var turnout = AvailableTurnouts[i];

            // If we have already used this button, then remove it from the Selectable ones
            // ---------------------------------------------------------------------------
            var found = TurnoutActions.Any(btn => btn.Id == turnout);
            if (TurnoutActions.Any(btn => btn.Id == turnout) && turnout != activeTurnout) {
                SelectableTurnouts.Remove(turnout);
            } else {
                // Otherwise add it to the Selectable ones as it may have been removed
                // -------------------------------------------------------------------
                if (!SelectableTurnouts.Contains(turnout)) {
                    SelectableTurnouts.Add(turnout);
                }
            }
        }
        OnPropertyChanged(nameof(TurnoutActions));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(NoDataText));
        OnPropertyChanged(nameof(SelectableTurnouts));
        OnPropertyChanged(nameof(AvailableTurnouts));
        OnPropertyChanged(nameof(ControlHeight));
    }
}