using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DCCPanelController.Model;

/// <summary>
///     This is used to Store the Data and Settings
/// </summary>
public class Storage {
    public Settings Settings { get; set; } = new();
    public ObservableCollection<Turnout> Turnouts { get; set; } = [];
    public ObservableCollection<Route> Routes { get; set; } = [];
    public ObservableCollection<Panel> Panels { get; set; } = [];

    public void ReOrderPanels() {
        if (Panels.Count <= 1) {
            foreach (var panel in Services.SampleData.Panels.DemoData()) {
                Panels.Add(panel);
            }
        }

        for (var index = 0; index < Panels.Count; index++) {
            Panels[index].SortOrder = index + 1;
        }
    }
}
