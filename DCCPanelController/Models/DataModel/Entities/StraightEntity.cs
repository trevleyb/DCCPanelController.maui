using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class StraightEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public StraightEntity() { } // Override the Straight Entity so it overlays anything else

    public StraightEntity(Panel panel) : this() => Parent = panel;

    public StraightEntity(StraightEntity entity) : base(entity) { }

    [ObservableProperty] [property: Editable("Track Decorator", "Select how this track is terminated.", 4, "Track")]
    private TrackStyleEnum _trackStyle = TrackStyleEnum.Normal;

    [ObservableProperty] [property: Editable("Decorator Color", "Color for the terminator if defined.", 5, "Color")]
    private Color? _terminatorColor;
    
    [JsonIgnore] public override EntityConnections Connections =>
        TrackStyle switch {
            TrackStyleEnum.Normal     => EntityConnections.TrackPatterns.StraightTrack,
            TrackStyleEnum.Tunnel     => EntityConnections.TrackPatterns.StraightTrack,
            TrackStyleEnum.Bridge     => EntityConnections.TrackPatterns.StraightTrack,
            TrackStyleEnum.Platform   => EntityConnections.TrackPatterns.StraightTrack,
            TrackStyleEnum.Terminator => EntityConnections.TrackPatterns.TerminatorTrack,
            TrackStyleEnum.Lines      => EntityConnections.TrackPatterns.TerminatorTrack,
            TrackStyleEnum.Arrow      => EntityConnections.TrackPatterns.TerminatorTrack,
            TrackStyleEnum.Rounded    => EntityConnections.TrackPatterns.TerminatorTrack,
            _                         => EntityConnections.TrackPatterns.StraightTrack,
        };

    [JsonIgnore] public override string EntityName =>
        TrackStyle switch {
            TrackStyleEnum.Normal     => "Straight",
            TrackStyleEnum.Tunnel     => "Tunnel",
            TrackStyleEnum.Bridge     => "Bridge",
            TrackStyleEnum.Platform   => "Platform",
            TrackStyleEnum.Terminator => "Terminator",
            TrackStyleEnum.Lines      => "Lines",
            TrackStyleEnum.Arrow      => "Arrow",
            TrackStyleEnum.Rounded    => "Rounded",
            _                         => "Track"
        };

    [JsonIgnore] public override string EntityDescription => TrackStyle switch {
        TrackStyleEnum.Normal     => "Straight Track",
        TrackStyleEnum.Tunnel     => "Tunnel Entrance",
        TrackStyleEnum.Bridge     => "Bridge Side Track",
        TrackStyleEnum.Platform   => "Platform Track",
        TrackStyleEnum.Terminator => "Terminator End",
        TrackStyleEnum.Lines      => "Lines Continuation",
        TrackStyleEnum.Arrow      => "Arrow Continuation",
        TrackStyleEnum.Rounded    => "Rounded End",
        _                         => "Normal Track"
    };

    [JsonIgnore] public override string EntityInformation =>
        TrackStyle switch {
            TrackStyleEnum.Normal     => "The **straight** is a straight piece of track. ",
            TrackStyleEnum.Tunnel     => "The **tunnel** represents the entrance or exit of a tunnel. Use it with Dashed Track.",
            TrackStyleEnum.Bridge     => "The **bridge track** shows an indicator to represent that the track is over a bridge.",
            TrackStyleEnum.Platform   => "The **platform** shows an indicator, either on top or bottom, to represent a platform at a station. ",
            TrackStyleEnum.Terminator => "The **terminator**  (or **buffer stop**) is used to indicate the end of a track. ",
            TrackStyleEnum.Lines      => "The **lines continuation** indicates that the track continues on another panel or page.",
            TrackStyleEnum.Arrow      => "The **arrow continuation** indicates that the track continues on another panel or page.",
            TrackStyleEnum.Rounded    => "The **rounded track** is a rounded end to the track.",
            _                         => "A **normal track** is a straight piece of track. ",
        };

    [JsonIgnore] protected override int RotationFactor => 45;

    public override Entity Clone() => new StraightEntity(this);
}