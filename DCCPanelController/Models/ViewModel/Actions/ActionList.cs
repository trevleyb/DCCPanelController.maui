namespace DCCPanelController.Models.ViewModel.Actions;

public enum ActionType {
    Button,
    Turnout
}

public class ActionList {
    private readonly Dictionary<ActionType, List<string>> _actioned = new();

    public bool IsActioned(ActionType actionType, string actionID) {
        if (_actioned.TryGetValue(actionType, out var actionList)) {
            if (actionList.Contains(actionID)) return true;
            actionList.Add(actionID);
            return false;
        }

        _actioned.Add(actionType, [actionID]);
        return false;
    }
}