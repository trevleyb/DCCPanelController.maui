using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;

namespace DCCPanelController.Components.Symbols;

public partial class SymbolViewModel : BaseViewModel {
    
    [ObservableProperty] 
    private string _name = string.Empty;

    [ObservableProperty]
    private ImageSource? _image;
    
    [ObservableProperty]
    private TrackTypesEnum _trackType;

}