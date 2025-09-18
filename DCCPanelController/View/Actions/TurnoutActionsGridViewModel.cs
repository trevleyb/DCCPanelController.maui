using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class TurnoutActionsGridViewModel : ActionsGridViewModel<TurnoutAction, TurnoutActions>, IActionsGridViewModel {
    public TurnoutActionsGridViewModel(TurnoutActions actions, ActionsContext context, List<string> availableTurnouts, Action? changedAction) : base(context, availableTurnouts, changedAction) {
        TurnoutPanelActions = actions;
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(PanelActions)) {
                OnPropertyChanged(nameof(TurnoutPanelActions));
            }
        };
        CleanupInvalidActions();
        UpdateSelectableItems();
    }

    public TurnoutActions TurnoutPanelActions { get; init; }
    protected override TurnoutActions PanelActions => TurnoutPanelActions;
    protected override string ItemTypeName => "Turnout";

    protected override TurnoutAction CreateNewAction(string id) => new() {
        ActionID = id,
        WhenClosed = TurnoutStateEnum.Closed,
        WhenThrown = TurnoutStateEnum.Thrown,
    };

    protected override string GetActionId(TurnoutAction action) => action.ActionID;
}