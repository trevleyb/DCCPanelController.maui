using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages;
using DCCPanelController.View.PropertyPages.Attributes;

namespace DCCPanelController.Model.DataModel.Tracks;

public abstract partial class TrackTurnout : Track {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private Actions<ButtonStateEnum> _buttonActions = [];
    [ObservableProperty] private Actions<TurnoutStateEnum> _turnoutActions = [];
    [ObservableProperty] private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;
    [ObservableProperty] private Color? _trackColor = null;
}