using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class StraightEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public StraightEntity() => Layer = 6; // Override the Straight Entity so it overlays anything else

    public StraightEntity(Panel panel) : this() => Parent = panel;

    public StraightEntity(StraightEntity entity) : base(entity) { }

    [ObservableProperty] [property: Editable("Track Decorator Style", "Select how this track is terminated.", 4, "Track")]
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
    
    public override string EntityName => "Track";
    public override string EntityDescription => "Straight Track";
    public override string EntityInformation =>
        "The **straight** is a straight piece of track .";

    [JsonIgnore] protected override int RotationFactor => 45;

    public override Entity Clone() => new StraightEntity(this);
}