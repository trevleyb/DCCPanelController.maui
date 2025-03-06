using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DCCPanelController.Model.DataModel;

/// <summary>
///     This is used to Store the Data and Settings
/// </summary>
[JsonSerializable(typeof(Storage))]
public class Storage {
    public Settings Settings { get; set; } = new();
    public Panels Panels { get; set; } = [];
    public ObservableCollection<Turnout> Turnouts { get; set; } = [];
    public ObservableCollection<Route> Routes { get; set; } = [];
}