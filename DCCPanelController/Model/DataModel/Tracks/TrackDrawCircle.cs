using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackCircle : Track {
    public override string Name => "Circle";
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;
    [ObservableProperty] private Color _borderColor = Colors.Black;
    [ObservableProperty] private int _borderWidth = 1;
    [ObservableProperty] private double _opacity = 1;
}