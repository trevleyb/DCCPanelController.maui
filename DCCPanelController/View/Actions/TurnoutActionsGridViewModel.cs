using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public class TurnoutActionsGridViewModel : ActionsGridViewModel<TurnoutAction, TurnoutActions>, IActionsGridViewModel {
    public TurnoutActionsGridViewModel(IActionEntity entity, ActionsContext context, List<string> availableTurnouts) : base(entity, context, availableTurnouts) {
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(PanelActions)) {
                OnPropertyChanged(nameof(TurnoutPanelActions));
            }
        };
    }

    public TurnoutActions TurnoutPanelActions => Entity.TurnoutPanelActions;
    protected override TurnoutActions PanelActions => TurnoutPanelActions;
    protected override string ItemTypeName => "Turnout";

    protected override TurnoutAction CreateNewAction(string id) {
        return new TurnoutAction {
            Id = id,
            WhenClosed = TurnoutStateEnum.Closed,
            WhenThrown = TurnoutStateEnum.Thrown
        };
    }

    protected override string GetActionId(TurnoutAction action) {
        return action.Id;
    }
}