using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.ViewModel;

public partial class BaseViewModel : ObservableObject {

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy = false;

    [ObservableProperty] private string _title = string.Empty;

    
    [ObservableProperty] private bool _isRefreshing;

    
    public bool IsNotBusy => !IsBusy;
}
