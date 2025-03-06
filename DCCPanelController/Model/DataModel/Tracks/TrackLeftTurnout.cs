using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages;
using DCCPanelController.View.PropertyPages.Attributes;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackLeftTurnout : TrackTurnout {
    public override string Name => "Left Turnout";
    public TrackLeftTurnout() {}
    public TrackLeftTurnout(TrackLeftTurnout track) : base(track) {}
}