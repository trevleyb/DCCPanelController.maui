using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGridViewModel : ObservableObject {
    [ObservableProperty] private ActionsContext _actionContext;
    [ObservableProperty] private TurnoutActions _turnoutPanelActions;

    [ObservableProperty] List<string> _availableTurnouts;
    [ObservableProperty] List<string> _selectableTurnouts;

    public TurnoutActionsGridViewModel(TurnoutActions turnoutPanelActions, ActionsContext context, List<string> availableTurnouts) {
        ActionContext = context;
        AvailableTurnouts = availableTurnouts;
        SelectableTurnouts = new List<string>(availableTurnouts);
        TurnoutPanelActions = turnoutPanelActions;
        UpdateSelectableTurnouts();
        OnPropertyChanged(nameof(IsTurnoutContext));
        OnPropertyChanged(nameof(IsButtonContext));
        OnPropertyChanged(nameof(ControlHeight));
    }

    public bool IsTurnoutContext => ActionContext == ActionsContext.Turnout;
    public bool IsButtonContext => ActionContext == ActionsContext.Button;

    public bool IsGridVisible => TurnoutPanelActions.Count > 0;
    public bool IsAddButtonEnabled => SelectableTurnouts.Count > 0;
    public double ControlHeight => 40 + TurnoutPanelActions.Count * 40;

    public string NoDataText {
        get {
            if (AvailableTurnouts.Count == 0) return "No Turnouts have been defined. ";
            if (TurnoutPanelActions.Count == 0) return "Use the + key to add a turnout action.";
            if (SelectableTurnouts.Count == 0) return "All defined turnouts have been assigned.";
            return "";
        }
    }

    [RelayCommand]
    private void AddRow() {
        if (SelectableTurnouts.Count > 0) {
            TurnoutPanelActions.Add(new TurnoutAction { Id = SelectableTurnouts[0], WhenClosed = TurnoutStateEnum.Closed, WhenThrown = TurnoutStateEnum.Thrown, Cascade = false });
        }

        UpdateSelectableTurnouts();
    }

    [RelayCommand]
    private void RemoveRow(TurnoutAction panelAction) {
        TurnoutPanelActions.Remove(panelAction);
        UpdateSelectableTurnouts();
    }

    [RelayCommand]
    private void IdValueChanged(string id) {
        UpdateSelectableTurnouts();
    }

    // Build a collection of all available turnouts that could be selected.
    // However, if this control IS A Turnout then we can select this one so exclude it. 
    private ObservableCollection<string> FindAvailableTurnouts(TurnoutEntity turnout) {
        var foundTurnouts = new ObservableCollection<string>();
        if (turnout is { Parent: { Entities: { } entities } }) {
            foreach (var trk in entities) {
                if (trk is TurnoutEntity trackTurnout) {
                    if (turnout != null && trackTurnout.Id == turnout.Id) continue;
                    if (string.IsNullOrWhiteSpace(trackTurnout.Id)) continue;
                    foundTurnouts.Add(trackTurnout.Id);
                }
            }
        }
        return foundTurnouts;
    }

    public void UpdateSelectableTurnouts(string? activeTurnout = "") {
        for (var i = AvailableTurnouts.Count - 1; i >= 0; i--) {
            var turnout = AvailableTurnouts[i];

            if (TurnoutPanelActions.Any(btn => btn.Id == turnout) && turnout != activeTurnout) {
                SelectableTurnouts.Remove(turnout);
            } else {
                // Otherwise add it to the Selectable ones as it may have been removed
                // -------------------------------------------------------------------
                if (!SelectableTurnouts.Contains(turnout)) {
                    SelectableTurnouts.Add(turnout);
                }
            }
        }

        OnPropertyChanged(nameof(TurnoutPanelActions));
        OnPropertyChanged(nameof(SelectableTurnouts));
        OnPropertyChanged(nameof(AvailableTurnouts));
        OnPropertyChanged(nameof(ControlHeight));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(NoDataText));
    }
}