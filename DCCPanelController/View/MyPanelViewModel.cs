using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Properties.PanelProperties;

// For IPropertiesViewModel

namespace DCCPanelController.View.Properties.Testing;

public class MyPanelViewModel : IPropertiesViewModel {
    public string Title => "Panel Settings";
    private string _panelName;

    public string PanelName {
        get => _panelName;
        set => _panelName = value; // Implement INotifyPropertyChanged if UI binds directly
    }

    public MyPanelViewModel(string initialName) {
        _panelName = initialName;
    }

    public Task ApplyChangesAsync() {
        Console.WriteLine($"Applying changes: Panel Name = {PanelName}");
        return Task.CompletedTask;
    }

    public Microsoft.Maui.Controls.View CreatePropertiesView() {
        var panels = new Panels();
        var panel = panels.CreatePanel();
        panel.Id = "Test Panel";
        var propPage = new PanelPropertyPage(panel);
        return propPage;
    }
}