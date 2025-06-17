using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGridViewModel : ObservableObject {
    [ObservableProperty] private ActionsContext _actionContext;
    [ObservableProperty] private List<string> _availableTurnouts;
    [ObservableProperty] private List<string> _selectableTurnouts;

    private readonly IActionEntity _entity;
    public TurnoutActions TurnoutPanelActions => _entity.TurnoutPanelActions;

    public TurnoutActionsGridViewModel(IActionEntity entity, ActionsContext context, List<string> availableTurnouts) {
        _entity = entity;
        ActionContext = context;
        AvailableTurnouts = availableTurnouts;
        SelectableTurnouts = BuildSelectableTurnouts();
        RaisePropertiesChanged();
    }

    public bool IsTurnoutContext => ActionContext == ActionsContext.Turnout;
    public bool IsButtonContext => ActionContext == ActionsContext.Button;
    public bool IsGridVisible => TurnoutPanelActions.Count > 0;
    public bool IsAddButtonEnabled => SelectableTurnouts.Count > 0;
    public double ControlHeight => 40 + (TurnoutPanelActions.Count * 40);

    public string NoDataText {
        get {
            if (AvailableTurnouts.Count == 0) return "No available Turnouts defined.";
            if (TurnoutPanelActions.Count == 0 && IsAddButtonEnabled) return "Use the + key to add a turnout action.";
            if (SelectableTurnouts.Count == 0) return "All available turnouts have been assigned.";
            return "Use the + key to add more turnout actions.";

        }
    }

    private void RaisePropertiesChanged() {
        OnPropertyChanged(nameof(TurnoutPanelActions));
        OnPropertyChanged(nameof(ControlHeight));
        OnPropertyChanged(nameof(NoDataText));
        OnPropertyChanged(nameof(IsAddButtonEnabled));
        OnPropertyChanged(nameof(IsGridVisible));
        OnPropertyChanged(nameof(IsTurnoutContext));
        OnPropertyChanged(nameof(IsButtonContext));
    }
    
    [RelayCommand]
    private void AddRow() {
        if (BuildSelectableTurnouts().Count > 0) {
            TurnoutPanelActions.Add(new TurnoutAction { Id = BuildSelectableTurnouts()[0], WhenClosed = TurnoutStateEnum.Closed, WhenThrown = TurnoutStateEnum.Thrown});
        }
        SelectableTurnouts = BuildSelectableTurnouts();
        RaisePropertiesChanged();
    }

    [RelayCommand]
    private void RemoveRow(TurnoutAction panelAction) {
        TurnoutPanelActions.Remove(panelAction);
        SelectableTurnouts = BuildSelectableTurnouts();
        RaisePropertiesChanged();
    }

    [RelayCommand]
    private void IdValueChanged(string id) {
        RaisePropertiesChanged();    
    }

    // Build a collection of all available turnouts that could be selected.
    // However, if this control IS A Turnout then we can select this one so exclude it. 
    private ObservableCollection<string> FindAvailableTurnouts(TurnoutEntity turnout) {
        var foundTurnouts = new ObservableCollection<string>();
        if (turnout is { Parent: { Entities: { } entities } }) {
            foreach (var trk in entities) {
                if (trk is TurnoutEntity trackTurnout) {
                    if (turnout != null && trackTurnout.TurnoutID == turnout.TurnoutID) continue;
                    if (string.IsNullOrWhiteSpace(trackTurnout.TurnoutID)) continue;
                    foundTurnouts.Add(trackTurnout.TurnoutID);
                }
            }
        }
        return foundTurnouts;
    }

    public List<string> BuildSelectableTurnouts(string? activeTurnout = "") {
        var selectableTurnouts = new List<string>(AvailableTurnouts);
        for (var i = AvailableTurnouts.Count - 1; i >= 0; i--) {
            var turnout = AvailableTurnouts[i];

            if (TurnoutPanelActions.Any(btn => btn.Id == turnout) && turnout != activeTurnout) {
                if (selectableTurnouts.Contains(turnout)) selectableTurnouts.Remove(turnout);
            } else {
                // Otherwise add it to the Selectable ones as it may have been removed
                // -------------------------------------------------------------------
                if (!selectableTurnouts.Contains(turnout)) {
                    selectableTurnouts.Add(turnout);
                }
            }
        }
        return selectableTurnouts;
    }
}