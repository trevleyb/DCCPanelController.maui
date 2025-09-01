using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel;

public partial class Route : ObservableObject {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private bool _isEditable;
    [ObservableProperty] private bool _isModified;
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    [JsonIgnore]
    public string DisplayFormat => $"{Name} ({Id})";

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Route() { }

}