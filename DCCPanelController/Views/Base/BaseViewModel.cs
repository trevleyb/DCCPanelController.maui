using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Views.Base;

public partial class BaseViewModel : ObservableObject {
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotRefreshing))]
    private bool _isRefreshing;

    public bool IsNotBusy => !IsBusy;
    public bool IsNotRefreshing => !IsRefreshing;
}