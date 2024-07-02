using System.Text.Json.Serialization;

namespace DCCPanelController.Model;

/// <summary>
/// Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public class Panel {
    public string Id { get; set; } = "new";
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public string? ImageAsBase64 { get; set; } = null;
    public List<TurnoutPoint> Turnouts { get; set; } = [];
    public ImageSource? Image {
        get {
            if (string.IsNullOrEmpty(ImageAsBase64)) return null;
            var base64Stream = Convert.FromBase64String(ImageAsBase64);
            return ImageSource.FromStream(() => new MemoryStream(base64Stream));
        }
    }
}

//[JsonSerializable(typeof(List<Panel>))]
//internal sealed partial class PanelStateContext : JsonSerializerContext{ }
