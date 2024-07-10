using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class Symbol : ObservableObject {
    
    [ObservableProperty] private ImageSource _image;
    [ObservableProperty] private string _name;
    
}