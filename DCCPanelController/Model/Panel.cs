using System.Text.Json.Serialization;

namespace RailwayPanel.Model;

/// <summary>
/// Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public class Panel {
    public int Order { get; set; }
    public string Name { get; set; }
    public Image Image { get; set; }
}