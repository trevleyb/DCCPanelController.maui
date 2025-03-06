using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.DataModel.Tracks;
public partial class TrackRectangle : Track {
    public override string Name => "Rectangle";
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;
    [ObservableProperty] private Color _borderColor = Colors.Black;
    [ObservableProperty] private int _borderWidth  = 1;
    [ObservableProperty] private double _opacity = 1;
}