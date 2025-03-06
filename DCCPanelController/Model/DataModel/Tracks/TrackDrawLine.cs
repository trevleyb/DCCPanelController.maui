using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.DataModel.Tracks;
public partial class TrackLine : Track {
    public override string Name => "Line";
    [ObservableProperty] private Color _lineColor = Colors.Black;
    [ObservableProperty] private int _lineWidth = 3;
    [ObservableProperty] private double _opacity = 1;

    public TrackLine() { }
    public TrackLine(TrackLine track) : base(track) {
        LineColor = track.LineColor;
        LineWidth = track.LineWidth;
        Opacity = track.Opacity;
    }
}