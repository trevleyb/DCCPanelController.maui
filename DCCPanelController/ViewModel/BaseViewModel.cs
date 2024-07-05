using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DCCPanelController.ViewModel;
    
public partial class BaseViewModel : ObservableValidator {

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy = false;
    
    [ObservableProperty] 
    private bool _isRefreshing;
    
    public bool IsNotBusy => !IsBusy;
    
}
