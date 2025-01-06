namespace DCCPanelController.Helpers.EditableProperties;

[AttributeUsage(AttributeTargets.Property)]
public class EditableActionsPropertyAttribute : EditableProperty {
    
    private bool _isButtonContext;
    private bool _isTurnoutContext;

    public bool IsButtonContext {
        get => _isButtonContext;
        set {
            _isButtonContext = value;
            _isTurnoutContext = !value;
        }
    }

    public bool IsTurnoutContext {
        get => _isTurnoutContext;
        set {
            _isTurnoutContext = value;
            _isButtonContext = !value;
        }
    }
}