using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Actions;

public partial class ButtonActionsGridViewModel : ActionsGridViewModel<ButtonAction, ButtonActions>, IActionsGridViewModel {
   public ButtonActionsGridViewModel(ButtonActions actions, ActionsContext context, List<string> availableButtons, Action? changedAction) : base(context, availableButtons, changedAction) {
        ButtonPanelActions = actions;
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(PanelActions)) {
                OnPropertyChanged(nameof(ButtonPanelActions));
            }
        };
        CleanupInvalidActions();
        UpdateSelectableItems();
    }

    public ButtonActions ButtonPanelActions { get; init; }
    protected override ButtonActions PanelActions => ButtonPanelActions;
    protected override string ItemTypeName => "Button";

    protected override ButtonAction CreateNewAction(string id) => new() {
        ActionID = id,
        WhenOn = ButtonStateEnum.On,
        WhenOff = ButtonStateEnum.Off,
    };

    protected override string GetActionId(ButtonAction action) => action.ActionID;
}