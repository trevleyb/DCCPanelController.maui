using System.Text.Json.Serialization;

namespace RailwayPanel.Model;
/// <summary>
/// Represents a Turnout with its current state
/// </summary>
public class TurnoutState(string id, string name, TurnoutStateEnum state = TurnoutStateEnum.Unknown) {
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public TurnoutStateEnum State { get; set; } = state;
}

public enum TurnoutStateEnum {
    Closed, 
    Thrown, 
    Unknown
} 

