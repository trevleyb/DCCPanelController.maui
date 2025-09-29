using System.ComponentModel;
using DCCPanelController.Clients;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {
    private readonly ILogger<SettingsPage>  _logger;
    private readonly SettingsPageViewModel? _pageViewModel;

    public SettingsPage(ILogger<SettingsPage> logger, SettingsPageViewModel pageViewModel) {
        _logger = logger;
        _pageViewModel = pageViewModel;
        ArgumentNullException.ThrowIfNull(_pageViewModel);
        BindingContext = _pageViewModel;
        InitializeComponent();
        _pageViewModel.IsDirty = false;
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args) {
        base.OnNavigatedFrom(args);
        if (_pageViewModel is { IsDirty: true } vm) await vm.SaveSettingsAsync();
    }
    
    public void BottomSheetOnStateChanged(object? sender, StateChangedEventArgs e) {
        if (e.NewState is BottomSheetState.Hidden or BottomSheetState.Collapsed) {
            if (sender is SfBottomSheet bottomSheet) bottomSheet.Content = null!;
            _pageViewModel?.IsNavigationDrawerOpen = false;
        } else {
            _pageViewModel?.IsNavigationDrawerOpen = true;
        }
    }

    private void IsProfileActiveChanged(object? sender, EventArgs eventArgs) {
        _pageViewModel?.MarkActiveProfileDefault();
        OnPropertyChanged(nameof(_pageViewModel.IsProfileDefault));
    }
}