using DCCPanelController.Models.DataModel.Entities.Actions;

namespace DCCPanelController.Models.DataModel.Entities.Interfaces;

public interface IActionEntity {
    public ButtonActions ButtonPanelActions { get; set; }
    public TurnoutActions TurnoutPanelActions { get; set; }
    public void CloneActionsInto(IActionEntity entity);
}