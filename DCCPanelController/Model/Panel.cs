using System.Text.Json.Serialization;

namespace DCCPanelController.Model;

/// <summary>
/// Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public class Panel {
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public Image? Image { get; set; }
}

//[JsonSerializable(typeof(List<Panel>))]
//internal sealed partial class PanelStateContext : JsonSerializerContext{ }
