using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DCCPanelController.Model;

/// <summary>
/// This is used to Store the Data and Settings
/// </summary>
public class Storage {
    public Settings Settings { get; set; } = new();
    public ObservableCollection<Turnout> Turnouts { get; set; } = new();
    public ObservableCollection<Route> Routes { get; set; } = new();
    public ObservableCollection<Panel> Panels { get; set; } = new();

    public void ReOrderPanels() {
        for (var index = 0; index < Panels.Count; index++) {
            Panels[index].SortOrder = index + 1;
        }
    }
}

[JsonSerializable(typeof(Storage))]
internal sealed partial class StorageContext : JsonSerializerContext { }