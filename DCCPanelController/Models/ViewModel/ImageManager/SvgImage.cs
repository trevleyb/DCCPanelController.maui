using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.ViewModel.ImageManager;

public partial class SvgImage : ObservableObject {
    private SvgImageManager? _imageManager;
    [ObservableProperty] private string _filename = string.Empty;
    [ObservableProperty] private int _rotation = 0;
    [ObservableProperty] private ConnectionType[] _connections = SvgConnections.NoConnections;

    public ConnectionType GetConnection(int index) => Connections[index];
    
    private SvgImageManager ImageManager => _imageManager ??= new SvgImageManager(Filename);
    public ImageSource ImageSource => ImageManager.ImageSource;

    public void SetAttribute(SvgElementType elementType, Color color) {
        ImageManager.SetAllAttributeValues(elementType, color.ToArgbHex());
    }
}