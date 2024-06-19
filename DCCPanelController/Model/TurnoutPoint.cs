namespace RailwayPanel.Model;

/// <summary>
/// A TurnoutPoint is a point on a Panel that represents a Turnout and how it controls
/// the layout. It defines an ID for the Button, and what it controls when clicked or
/// disabled. 
/// </summary>
public class TurnoutPoint {
    public string Id { get; set; }
    public string Name { get; set; }
}