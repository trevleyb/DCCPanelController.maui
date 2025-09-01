namespace DCCPanelController.View.Properties;

public interface IPropertyPage {
    Task ApplyChangesAsync();
    ContentView CreatePropertiesView();
}