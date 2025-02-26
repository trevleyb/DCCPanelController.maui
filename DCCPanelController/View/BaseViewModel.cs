using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.View;

public partial class BaseViewModel : ObservableValidator {
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    public bool IsNotBusy => !IsBusy;
}