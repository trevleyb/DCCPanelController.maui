using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Components;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;
 
public partial class OperateViewModel : Base.ConnectionViewModel {
    [ObservableProperty] private int _currentPanelIndex;
    [ObservableProperty] private bool _showGrid;
    [ObservableProperty] private bool _showPath;
    [ObservableProperty] private Panel? _activePanel;
    
    [NotifyPropertyChangedFor(nameof(IsNotMaximized))]
    [ObservableProperty] private bool _isMaximized;
    public bool IsNotMaximized => !IsMaximized;
    
    [NotifyPropertyChangedFor(nameof(HideWelcomePage))]
    [ObservableProperty] private bool _showWelcomePage;
    public bool HideWelcomePage => !ShowWelcomePage;

    public string VersionNumber => VersionInfo.Version; 
    public static string IconMinimize => "minimize_2";
    public static string IconMaximize => "maximize_2";
    
    public ObservableCollection<Panel>? Panels { get; private set; }
    public Color PanelBackgroundColor => ActivePanel?.PanelBackgroundColor ?? Colors.White;
    public Color DisplayBackgroundColor => ActivePanel?.DisplayBackgroundColor ?? Colors.White;
    public string ProfileName => _profileService.ActiveProfile?.ProfileName ?? "No Profile";
    
    public bool HasPanels => Panels?.Any() == true;
    public bool HasNoPanels => !HasPanels;

    private ILogger<OperateViewModel> _logger;
    private readonly ProfileService _profileService;
    
    public OperateViewModel(ILogger<OperateViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        _profileService.ActiveProfileChanged += (_, _) => OnProfileChanged();
        PropertyChanged += OnPropertyChanged;
        OnProfileChanged();
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e?.PropertyName == nameof(CurrentPanelIndex)) {
            await SelectPanelAsync(CurrentPanelIndex);
        }
    }

    public async void OnProfileChanged() {
        Console.WriteLine("Profile has changed to: " + _profileService.ActiveProfile?.ProfileName);
        Panels = null;
        var profile = _profileService.ActiveProfile;
        Panels = profile?.Panels ?? throw new ApplicationException($"OperateViewModel: Panels Collection should not be empty.");
        ShowWelcomePage = Panels.Count <= 0 || (profile?.Settings?.ShowWelcomePage ?? true);
        await SelectPanelAsync(0);
    }

    [RelayCommand]
    private async Task SwitchActiveProfileAsync() {
        var choices = _profileService.GetProfileNamesWithDefault();
        var index = await ProfileSelector.ShowProfileSelector(choices);
        if (index is {} selectedProfile and >= 0) await _profileService.SwitchProfileByIndexAsync(selectedProfile);
    }
    
    [RelayCommand]
    private async Task ReselectActivePanelAsync() {
        try {
            await SelectPanelAsync(CurrentPanelIndex);
        } catch {
            Console.WriteLine("Error reselecting panel:  probably should never happen.");
        }
    }

    
    [RelayCommand]
    private async Task SwipeLeftAsync() {
        ShowWelcomePage = Panels?.Count <= 0 ? true : false;
        if (Panels?.Any() == true) {
            await SelectPanelAsync(CurrentPanelIndex <= 0 ? Panels.Count - 1 : CurrentPanelIndex - 1);
        }
    }

    [RelayCommand]
    private async Task SwipeRightAsync() {
        ShowWelcomePage = Panels?.Count <= 0 ? true : false;
        if (Panels?.Any() == true) {
            await SelectPanelAsync((CurrentPanelIndex + 1) % Panels.Count);
        }
    }

    [RelayCommand]
    public async Task SelectPanelAsync(int index) {
        CurrentPanelIndex = index;
        try {
            if (index < 0) index = 0;
            if (Panels?.Count > 0 && index < Panels.Count) {
                ActivePanel = null;
                ActivePanel = Panels[index];
            }
            ShowWelcomePage = Panels?.Count <= 0 ? true : false;
        } catch (Exception ex) {
            Console.WriteLine("Error selecting panel: " + ex.Message);
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
