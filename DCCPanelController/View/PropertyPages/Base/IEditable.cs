namespace DCCPanelController.View.PropertyPages.Base;

public interface IEditableAttribute {
    IView? CreateView(EditableDetails value);
}