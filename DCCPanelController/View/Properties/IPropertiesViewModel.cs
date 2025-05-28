using static DCCPanelController.View.Properties.PropertyDisplayService;

namespace DCCPanelController.View.Properties;

public interface IPropertiesViewModel {
    string Title { get; }
    Task ApplyChangesAsync();
    Microsoft.Maui.Controls.View CreatePropertiesView();
}