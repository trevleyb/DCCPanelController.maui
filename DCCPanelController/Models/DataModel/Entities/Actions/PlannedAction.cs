namespace DCCPanelController.Models.DataModel.Entities.Actions;

public enum ActionTargetKind { Button, Turnout }

public record PlannedAction(
    ActionTargetKind Kind,
    string EntityId, // stable id within the panel
    object Target,   // ButtonEntity or TurnoutEntity (strongly typed when used)
    object NewState  // ButtonStateEnum or TurnoutStateEnum
);
