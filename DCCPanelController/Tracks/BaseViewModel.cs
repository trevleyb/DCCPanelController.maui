using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Components;

public partial class BaseViewModel : ObservableObject {
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy = false;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsNotSelected))]
    private bool _isSelected;

    [ObservableProperty] 
    private bool _isRefreshing;
    
    public bool IsNotSelected => !IsSelected;
    public bool IsNotBusy => !IsBusy;
}