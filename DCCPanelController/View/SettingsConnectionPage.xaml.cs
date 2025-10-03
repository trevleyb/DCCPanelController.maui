using System.ComponentModel;
using System.Diagnostics;
using DCCPanelController.Clients;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class SettingsConnectionPage : ContentPage, INotifyPropertyChanged {
    private readonly ILogger<SettingsPage>  _logger;
    private readonly SettingsConnectionViewModel? _pageViewModel;

    public SettingsConnectionPage(ILogger<SettingsPage> logger, SettingsConnectionViewModel pageViewModel) {
        InitializeComponent();
        _logger = logger;
        _pageViewModel = pageViewModel;
        ArgumentNullException.ThrowIfNull(_pageViewModel);
        BindingContext = _pageViewModel;
        
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        if (_pageViewModel is not null) {
            _pageViewModel.SetActiveSettings();
            _pageViewModel.SelectedSegmentIndex = _pageViewModel.Settings?.ClientSettings?.Type switch {
                DccClientType.Simulator  => 0,
                DccClientType.Jmri       => 1,
                DccClientType.WiThrottle => 2,
                _                        => 0,
            };
            _pageViewModel.IsDirty = false;
        } else {
            Debug.WriteLine("PageViewModel is null");
        }
    }
    
    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args) {
        base.OnNavigatedFrom(args);
        if (_pageViewModel is { IsDirty: true } vm) await vm.SaveSettingsAsync();
    }
    
}