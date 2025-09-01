namespace DCCPanelController.Views.Properties;

public interface IPropertyPage {
    Task ApplyChangesAsync();
    ContentView CreatePropertiesView();
}