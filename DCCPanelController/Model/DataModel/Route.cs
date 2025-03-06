using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.DataModel.Tracks;

namespace DCCPanelController.Model.DataModel;

[JsonSerializable(typeof(Route))]
public partial class Route : ObservableObject {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Route() { }
}
