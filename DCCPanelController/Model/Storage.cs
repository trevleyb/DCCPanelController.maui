using System.Text.Json.Serialization;

namespace DCCPanelController.Model;

/// <summary>
/// This is used to Store the Data and Settings
/// </summary>
public class Storage {
    public Settings Settings { get; set; } = new();
    public List<Panel> Panels { get; set; } = new();
}

//[JsonSerializable(typeof(Storage))]
//internal sealed partial class StorageContext : JsonSerializerContext{ }
