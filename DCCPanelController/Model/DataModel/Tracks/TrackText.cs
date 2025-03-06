using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages.Attributes;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackText : Track {
    public override string Name => "Text Block";
    [ObservableProperty] private int _borderRadius = 0;
    [ObservableProperty] private int _borderWidth  = 0;
    [ObservableProperty] private int _fontSize = 8;
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private Color _textColor = Colors.White;
    [ObservableProperty] private Color _borderColor = Colors.Transparent;
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;
    [ObservableProperty] private FontWeight _fontWeight = FontWeight.Regular;
    [ObservableProperty] private TextAlignment _horizontalJustification = TextAlignment.Center;
    [ObservableProperty] private TextAlignment _verticalJustification = TextAlignment.Center;
    
    public TrackText() {}

    public TrackText(TrackText track) : base(track) {
        BorderRadius = track.BorderRadius;
        BorderWidth = track.BorderWidth;
        TextColor = track.TextColor;
        BorderColor = track.BorderColor;
        BackgroundColor = track.BackgroundColor;
        FontSize = track.FontSize;
        FontWeight = track.FontWeight;
        Label = track.Label;
        HorizontalJustification = track.HorizontalJustification;
        VerticalJustification = track.VerticalJustification;

    }
}