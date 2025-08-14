using System.ComponentModel;
using CommunityToolkit.Maui.Storage;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.Models.DataModel.Repository;
using DCCPanelController.View.Settings;
using DCCPanelController.View.Settings.Jmri;
using DCCPanelController.View.Settings.Simulator;
using DCCPanelController.View.Settings.WiThrottle;
using Microsoft.Extensions.Logging;
using SelectionChangedEventArgs = Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {
    
    private readonly ILogger<SettingsPage> _logger;
    private readonly SettingsPageViewModel? _pageViewModel;

    public SettingsPage(ILogger<SettingsPage> logger, SettingsPageViewModel pageViewModel) {
        _logger = logger;
        _pageViewModel = pageViewModel;
        ArgumentNullException.ThrowIfNull(_pageViewModel);
        BindingContext = _pageViewModel;
        PropertyChanged += OnPropertyChanged;
        InitializeComponent();

        pageViewModel.SetActiveSettings();
        pageViewModel.SetCapabilities();
        SelectionSegmentControl.SelectedIndex = SetActiveSelectionSegment();
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) { }

    private void About_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void Instructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new HelpPage());
    }
    
    private void CheckChanged_Jmri(object? sender, CheckedChangedEventArgs e) {
        if (_pageViewModel is { IsJmriServer: true }) {
            SettingsView.Content = _pageViewModel.LoadSettingsPage();
        }
    }

    private void CheckChanged_WiThrottle(object? sender, CheckedChangedEventArgs e) {
        if (_pageViewModel is { IsWiThrottle: true }) {
            SettingsView.Content = _pageViewModel.LoadSettingsPage();
        }
    }

    private void CheckChanged_Simulator(object? sender, CheckedChangedEventArgs e) {
        if (_pageViewModel is { IsSimulator: true }) {
            SettingsView.Content = _pageViewModel.LoadSettingsPage();
        }
    }


    private void SettingsViewOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        OnPropertyChanged();
    }

    private int SetActiveSelectionSegment() {
        if (_pageViewModel is { Settings.ClientSettings: not null }) {
            return _pageViewModel.Settings.ClientSettings.Type switch {
                DccClientType.Simulator => 0,
                DccClientType.Jmri => 1,
                DccClientType.WiThrottle => 2,
                _ => 0,
            };
        }
        return 0;
    }
    
    private void SegmentSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (_pageViewModel is { Settings.ClientSettings: not null }) {
            var type = e.NewIndex switch {
                0 => DccClientType.Simulator,
                1 => DccClientType.Jmri,
                2 => DccClientType.WiThrottle,
                _ => DccClientType.Simulator,
            };
            _pageViewModel.SetActiveSettings(type);
            SettingsView.Content = _pageViewModel.LoadSettingsPage();
        }
    }
}