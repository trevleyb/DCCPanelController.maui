using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Symbols.TrackViewModels;

namespace DCCPanelController.Symbols;

public partial class SymbolViewModel : BaseViewModel {
    
    [ObservableProperty] 
    private string _name = string.Empty;

    [ObservableProperty]
    private ImageSource? _image;
    
    [ObservableProperty]
    private TrackTypesEnum _trackType;

}