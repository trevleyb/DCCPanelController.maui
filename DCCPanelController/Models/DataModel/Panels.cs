using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DCCPanelController.Models.DataModel;

public sealed class Panels : ObservableCollection<Panel> {

    public Panels() { }
    public Panel CreatePanel() => Add(new Panel(this));
    public Panel CreatePanelFrom(Panel panel) => Add(panel.Clone());
    new private Panel Add(Panel panel) {
        base.Add(panel);
        return panel;
    }
}