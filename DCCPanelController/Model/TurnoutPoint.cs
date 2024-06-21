using System.Text.Json.Serialization;

namespace DCCPanelController.Model;

/// <summary>
/// A TurnoutPoint is a point on a Panel that represents a Turnout and how it controls
/// the layout. It defines an ID for the Button, and what it controls when clicked or
/// disabled. 
/// </summary>
public class TurnoutPoint {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

//[JsonSerializable(typeof(List<TurnoutPoint>))]
//internal sealed partial class TurnoutPointContext : JsonSerializerContext{ }
