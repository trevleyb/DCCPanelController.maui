using DCCPanelController.Models.DataModel.Entities.Actions;

namespace DCCPanelController.Models.DataModel.Entities.Interfaces;

public interface IActionEntity {
    ButtonActions ButtonPanelActions { get; set; }
    TurnoutActions TurnoutPanelActions { get; set; }
    void CloneActionsInto(IActionEntity entity);
}