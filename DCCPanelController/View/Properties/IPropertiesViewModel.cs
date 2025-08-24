namespace DCCPanelController.View.Properties;

public interface IPropertiesViewModel {
    string Title { get; }
    Task ApplyChangesAsync();
    ContentView CreatePropertiesView();
}