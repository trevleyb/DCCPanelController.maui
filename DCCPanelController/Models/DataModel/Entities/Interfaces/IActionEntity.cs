using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.View.Actions;

namespace DCCPanelController.Models.DataModel.Entities.Interfaces;

public interface IActionEntity {
    ButtonActions ButtonPanelActions { get; set; }
    TurnoutActions TurnoutPanelActions { get; set; }
    abstract ActionsContext Context { get; }
    void CloneActionsInto(IActionEntity entity);
}