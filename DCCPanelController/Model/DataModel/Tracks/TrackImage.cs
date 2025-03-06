using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackImage : Track {
    public override string Name => "Image";
    [ObservableProperty] private int _borderRadius = 0;
    [ObservableProperty] private int _borderWidth = 0;
    [ObservableProperty] private Color _borderColor = Colors.Transparent;
    [ObservableProperty] private Aspect _aspectRatio = Aspect.AspectFit;
    [ObservableProperty] private string _image = string.Empty;
}
 