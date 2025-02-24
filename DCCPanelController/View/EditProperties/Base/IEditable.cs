namespace DCCPanelController.View.EditProperties.Base;

public interface IEditableAttribute {
    IView? CreateView(EditableDetails value);
}