using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel;

public partial class Route : ObservableObject, ITable {
    [ObservableProperty] private string?        _id;
    [ObservableProperty] private string?        _name;
    [ObservableProperty] private bool           _isEditable;
    [ObservableProperty] private bool           _isModified;
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Route() { }

    [JsonIgnore]
    public string DisplayFormat => $"{Name} ({Id})";
    
    public event EventHandler<RouteStateEnum>? StateChanged;
    partial void OnStateChanged(RouteStateEnum value) => StateChanged?.Invoke(this, value);

    public void ToggleState() {
        var newState = State switch {
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            _                       => RouteStateEnum.Active,
        };
        State = newState;
    }
}