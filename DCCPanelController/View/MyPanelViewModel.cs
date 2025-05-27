using DCCPanelController.Models.DataModel;
using DCCPanelController.View.PanelProperties;
using YourAppNamespace.ViewModels;

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
        // Logic to save the PanelName or other properties
        System.Diagnostics.Debug.WriteLine($"Applying changes: Panel Name = {PanelName}");
        return Task.CompletedTask;
    }

    public Microsoft.Maui.Controls.View CreatePropertiesView() {

        var panels = new Panels();
        var panel = panels.CreatePanel();
        panel.Id = "Test Panel";
        var propPage = new PanelPropertyBase(panel);
        return propPage;

        // var stackLayout = new VerticalStackLayout { Spacing = 10 };
        // stackLayout.Children.Add(new Label { Text = "Panel Name:" });
        //
        // var nameEntry = new Entry { Placeholder = "Enter panel name" };
        // nameEntry.SetBinding(Entry.TextProperty, new Binding(nameof(PanelName), source: this, mode: BindingMode.TwoWay));
        // stackLayout.Children.Add(nameEntry);
        //
        // // Add more controls for other properties here
        // return stackLayout;
    }
}