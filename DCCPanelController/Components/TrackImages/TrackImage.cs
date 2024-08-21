using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.Tracks;
using DCCPanelController.Components.Tracks.SVGManager;

namespace DCCPanelController.Components.TrackImages;

public partial class TrackImage : ObservableObject {

    [ObservableProperty] private string _name;
    [ObservableProperty] private int _rotation;

    [ObservableProperty] private SvgImageManager _imageManager;
    [ObservableProperty] private TrackConnections _connections;
    
    public TrackImage(string name, string imageName, int rotation, TrackConnections connections) {
        Name = name;
        Rotation = rotation;
        Connections = connections;
        ImageManager = new SvgImageManager(imageName);
    }

    public ImageSource? Image => ImageManager.Image;

    public void SetImageStyle() {}
    
    public void SetElementColorByName(string id, Color color, int opacity) {
        ImageManager.SetElementColor(id, color, opacity);
    }

    public void SetElementMainline(Color track) {
        //ImageManager.SetBorderColor(Colors.Black,100);
        //ImageManager.SetContinuationColor(Colors.Black,100);
        //ImageManager.SetTerminatorColor(Colors.Black,100);
        //ImageManager.SetTrackColor(track,100);
        //ImageManager.SetDivergingColor(track,100);
        OnPropertyChanged(nameof(Image));
    }
    
    public void SetElementBranchLine(Color track) {
        //ImageManager.SetBorderColor(Colors.Black,0);
        //ImageManager.SetContinuationColor(track,100);
        //ImageManager.SetTerminatorColor(track,100);
        //ImageManager.SetTrackColor(track,100);
        //ImageManager.SetDivergingColor(track,100);
        OnPropertyChanged(nameof(Image));
    }
    
}
