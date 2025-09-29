using System.ComponentModel;
using DCCPanelController.Clients;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class SettingsConnectionPage : ContentPage, INotifyPropertyChanged {
    private readonly ILogger<SettingsPage>  _logger;
    private readonly SettingsConnectionViewModel? _pageViewModel;

    public SettingsConnectionPage(ILogger<SettingsPage> logger, SettingsConnectionViewModel pageViewModel) {
        _logger = logger;
        _pageViewModel = pageViewModel;
        ArgumentNullException.ThrowIfNull(_pageViewModel);
        BindingContext = _pageViewModel;
        InitializeComponent();

        pageViewModel.SetActiveSettings();
        _pageViewModel.SelectedSegmentIndex = _pageViewModel.Settings?.ClientSettings?.Type switch {
            DccClientType.Simulator  => 0,
            DccClientType.Jmri       => 1,
            DccClientType.WiThrottle => 2,
            _                        => 0,
        };
        _pageViewModel.IsDirty = false;
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args) {
        base.OnNavigatedFrom(args);
        if (_pageViewModel is { IsDirty: true } vm) await vm.SaveSettingsAsync();
    }
    
}