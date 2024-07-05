using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

/// <summary>
/// This is used to Store the Data and Settings
/// </summary>
public class Storage {
    public Settings Settings { get; set; } = new();
    public ObservableCollection<Turnout> Turnouts { get; set; } = new();
    public ObservableCollection<Route> Routes { get; set; } = new();
    public ObservableCollection<Panel> Panels { get; set; } = new();
}

[JsonSerializable(typeof(Storage))]
internal sealed partial class StorageContext : JsonSerializerContext{ }
