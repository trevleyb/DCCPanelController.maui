using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Models.DataModel;

public sealed class Panels : ObservableCollection<Panel> {
    [JsonIgnore] public Profile? Profile { get; set; }

    public Panel CreatePanel() {
        return new Panel(this);
    }

    public Panel CreatePanel(string name) {
        var panel = new Panel(this);
        panel.Id = name;
        return panel;
    }

    public Panel CreatePanelFrom(Panel panel) {
        return panel.Clone();
    }

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
            return null;
        }
    }
}