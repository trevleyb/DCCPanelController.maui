namespace DCCPanelController.Models.DataModel.Entities.Interfaces;

public interface IActionEntity {
    public ButtonActions ButtonPanelActions { get; }
    public TurnoutActions TurnoutPanelActions { get; }
}