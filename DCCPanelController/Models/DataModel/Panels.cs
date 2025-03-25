using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Models.DataModel;

public sealed class Panels : ObservableCollection<Panel> {

    public Panels() { }
    public Panel CreatePanel() => new Panel(this);
    public Panel CreatePanelFrom(Panel panel) => panel.Clone();
    new private Panel Add(Panel panel) {
        base.Add(panel);
        return panel;
    }

    public Panel? UploadPanel(string panelAsJson) {
        try {
            var panel = JsonSerializer.Deserialize<Panel?>(panelAsJson, JsonOptions.Options);
            if (panel != null) {
                Add(panel);
                panel.Panels = this;
            }
            return panel;
        } catch {
            Console.WriteLine($"Could not upload panel: {panelAsJson}");
            return null;
        }
    }
}