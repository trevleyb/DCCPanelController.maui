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
        _pageViewModel.SelectedSegmentIndex = _pageViewModel.Settings?.ClientSettings?.Type switch {
            DCCPanelController.Clients.DccClientType.Simulator  => 0,
            DCCPanelController.Clients.DccClientType.Jmri       => 1,
            DCCPanelController.Clients.DccClientType.WiThrottle => 2,
            _                                                   => 0
        };
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) { }

    private void About_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void Instructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new HelpPage());
    }
}