using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public class ButtonActionsGridViewModel : ActionsGridViewModel<ButtonAction, ButtonActions>, IActionsGridViewModel {
    public ButtonActionsGridViewModel(IActionEntity entity, ActionsContext context, List<string> availableButtons) : base(entity, context, availableButtons) => PropertyChanged += (sender, args) => {
        if (args.PropertyName == nameof(PanelActions)) {
            OnPropertyChanged(nameof(ButtonPanelActions));
        }
    };

    public ButtonActions ButtonPanelActions => Entity.ButtonPanelActions;
    protected override ButtonActions PanelActions => ButtonPanelActions;
    protected override string ItemTypeName => "Button";

    protected override ButtonAction CreateNewAction(string id) => new() {
        Id = id,
        WhenOn = ButtonStateEnum.On,
        WhenOff = ButtonStateEnum.Off,
    };

    protected override string GetActionId(ButtonAction action) => action.Id;
}