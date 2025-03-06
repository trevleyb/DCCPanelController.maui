using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackImage : Track {
    public override string Name => "Image";
    [ObservableProperty] private int _borderRadius = 0;
    [ObservableProperty] private int _borderWidth = 0;
    [ObservableProperty] private Color _borderColor = Colors.Transparent;
    [ObservableProperty] private Aspect _aspectRatio = Aspect.AspectFit;
    [ObservableProperty] private string _image = string.Empty;

    public TrackImage() {}
    public TrackImage(TrackImage track) : base(track) {
        BorderRadius = track.BorderRadius;
        BorderWidth = track.BorderWidth;
        BorderColor = track.BorderColor;
        AspectRatio = track.AspectRatio;
        Image = track.Image;
    }
}
 