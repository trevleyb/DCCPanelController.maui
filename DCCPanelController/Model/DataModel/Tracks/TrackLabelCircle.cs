using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages.Attributes;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackLabelCircle : Track {
    public override string Name => "Circle Image";
    [ObservableProperty] private int _borderRadius = 0;
    [ObservableProperty] private int _borderWidth  = 0;
    [ObservableProperty] private int _fontSize = 8;
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private Color _textColor = Colors.White;
    [ObservableProperty] private Color _borderColor = Colors.Transparent;
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;
    [ObservableProperty] private FontWeight _fontWeight = FontWeight.Regular;
    [ObservableProperty] private double _opacity = 1;
    
    public TrackLabelCircle() {}
    public TrackLabelCircle(TrackLabelCircle track) : base( track) {
        BorderRadius = track.BorderRadius;
        BorderWidth = track.BorderWidth;
        BorderColor = track.BorderColor;
        BackgroundColor = track.BackgroundColor;
        FontSize = track.FontSize;
        FontWeight = track.FontWeight;
        Label = track.Label;
        TextColor = track.TextColor;
        Opacity = track.Opacity;
    }
}