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

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {
    private readonly SettingsPageViewModel? _pageViewModel;

    public SettingsPage(SettingsPageViewModel pageViewModel) {
        _pageViewModel = pageViewModel;
        ArgumentNullException.ThrowIfNull(_pageViewModel);
        BindingContext = _pageViewModel;
        PropertyChanged += OnPropertyChanged;
        InitializeComponent();

        pageViewModel.SetActiveSettings();
        pageViewModel.SetCapabilities();
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
}