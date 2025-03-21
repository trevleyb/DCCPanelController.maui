namespace DCCPanelController.Models.DataModel.Interfaces;

public interface IActionEntity {
    public ButtonActions ButtonPanelActions { get; }
    public TurnoutActions TurnoutPanelActions { get; }
}