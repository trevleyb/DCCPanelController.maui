using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using DCCPanelController.View.Components;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class OperateViewModel : ConnectionViewModel {
    private readonly ProfileService    _profileService;
    private readonly ConnectionService _connectionService;

    [ObservableProperty] private Panel? _activePanel;
    [ObservableProperty] private int    _currentPanelIndex;

    [NotifyPropertyChangedFor(nameof(IsNotMaximized))]
    [ObservableProperty] private bool _isMaximized;

    private                      ILogger<OperateViewModel>    _logger;
    [ObservableProperty] private ObservableCollection<Panel>? _panels;
    [ObservableProperty] private bool                         _showGrid;
    [ObservableProperty] private bool                         _showPath;

    [NotifyPropertyChangedFor(nameof(HideWelcomePage))]
    [ObservableProperty] private bool _showWelcomePage;

    public bool   HaveClosedWelcome;

    public OperateViewModel(ILogger<OperateViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _connectionService = connectionService;
        _profileService = profileService;
        _profileService.ActiveProfileChanged += (_, _) => OnProfileChanged();
        PropertyChanged += OnPropertyChanged;
        OnProfileChanged();
    }

    public bool IsNotMaximized => !IsMaximized;
    public bool HideWelcomePage => !ShowWelcomePage;

    public Color PanelBackgroundColor => ActivePanel?.PanelBackgroundColor ?? Colors.White;
    public Color DisplayBackgroundColor => ActivePanel?.DisplayBackgroundColor ?? Colors.White;
    public string ProfileName => _profileService.ActiveProfile?.ProfileName ?? "No Profile";
    public string VersionNumber => VersionInfo.Version;

    public bool HasPanels => Panels?.Any() == true;
    public bool HasNoPanels => !HasPanels;

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        try {
            if (e.PropertyName == nameof(CurrentPanelIndex)) {
                await SelectPanelAsync(CurrentPanelIndex);
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Error in OnPropertyChanged '{e.PropertyName}:{ex.Message}");
        }
    }

    public async void OnProfileChanged() {
        try {
            // Make sure we are fully disconnected from any existing service
            // ---------------------------------------------------------------
            await ConnectionService.DisconnectAsync();
            
            // Clear the existing panel and reload it from the profile service
            // ---------------------------------------------------------------
            ActivePanel = null;
            Panels = null;
            var profile = _profileService.ActiveProfile;
            if (profile is null) throw new NullReferenceException("Profile is not correctly set.");

            Panels = profile?.Panels ?? throw new ApplicationException("OperateViewModel: Panels Collection should not be empty.");
            ShowWelcomePage = Panels.Count <= 0 || (profile?.Settings?.ShowWelcomePage ?? true);
            HaveClosedWelcome = !ShowWelcomePage;
            if (profile!.Settings?.ConnectOnStartup == true) await _connectionService.ConnectAsync();
            await SelectPanelAsync(0);
            OnPropertyChanged(nameof(ProfileName));
        } catch (Exception e) {
            Debug.WriteLine("Error loading profile: " + e.Message);
        }
    }

    [RelayCommand]
    private async Task SwitchActiveProfileAsync() {
        var choices = _profileService.GetProfileNamesWithDefault();
        var index = await ProfileSelector.ShowProfileSelector("Select a Profile", choices);
        if (index is{ } selectedProfile and>= 0) {
            await _connectionService.DisconnectAsync();
            await _profileService.SwitchProfileByIndexAsync(selectedProfile);
        }
        HaveClosedWelcome = false;
    }

    [RelayCommand]
    public async Task ReselectActivePanelAsync() {
        try {
            await SelectPanelAsync(CurrentPanelIndex);
        } catch {
            Debug.WriteLine("Error reselecting panel:  probably should never happen.");
        }
    }

    [RelayCommand]
    private async Task SwipeLeftAsync() {
        HaveClosedWelcome = true;
        ShowWelcomePage = Panels?.Count <= 0;
        if (Panels?.Any() == true) {
            await SelectPanelAsync(CurrentPanelIndex <= 0 ? Panels.Count - 1 : CurrentPanelIndex - 1);
        }
    }

    [RelayCommand]
    private async Task SwipeRightAsync() {
        HaveClosedWelcome = true;
        ShowWelcomePage = Panels?.Count <= 0;
        if (Panels?.Any() == true) {
            await SelectPanelAsync((CurrentPanelIndex + 1) % Panels.Count);
        }
    }

    [RelayCommand]
    public async Task SelectPanelAsync(int index) {
        if (index < 0 || index >= Panels?.Count) index = 0;
        CurrentPanelIndex = index;
        try {
            ActivePanel = null;
            if (Panels?.Count > 0 && index < Panels.Count) {
                ActivePanel = Panels[index];
            }
            ShowWelcomePage = Panels?.Count <= 0 || !HaveClosedWelcome;
            RaiseAllProperties();
        } catch (Exception ex) {
            Debug.WriteLine("Error selecting panel: " + ex.Message);
        }
    }

    private void RaiseAllProperties() {
        OnPropertyChanged(nameof(ActivePanel));
        OnPropertyChanged(nameof(ShowWelcomePage));
        OnPropertyChanged(nameof(HideWelcomePage));
        OnPropertyChanged(nameof(HasPanels));
        OnPropertyChanged(nameof(HasNoPanels));
        OnPropertyChanged(nameof(PanelBackgroundColor));
        OnPropertyChanged(nameof(DisplayBackgroundColor));
    }
}