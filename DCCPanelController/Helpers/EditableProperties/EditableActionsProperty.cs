using DCCPanelController.ViewModel;

namespace DCCPanelController.Helpers.EditableProperties;

[AttributeUsage(AttributeTargets.Property)]
public class EditableActionsPropertyAttribute : EditableProperty {
    public ActionsContext ActionsContext { get; set; }
}