using System.Collections.ObjectModel;
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
    
    public ObservableCollection<PanelIndicator> PanelIndicators { get; set; } = [];
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
        CurrentPanelIndex = -1;
        OnProfileChanged();
    }
    
    private void OnProfileChanged() {
        Console.WriteLine("Profile has changed to: " + _profileService.ActiveProfile?.ProfileName);
        
        CurrentPanelIndex = -1;
        var profile = _profileService.ActiveProfile;
        Panels = [];
        Panels = profile?.Panels ?? throw new ApplicationException($"OperateViewModel: Panels Collection should not be empty.");
        ShowWelcomePage = Panels.Count <= 0 || (profile?.Settings.ShowWelcomePage ?? true);
        if (!ShowWelcomePage) SelectPanel(0);
        UpdatePanelIndicators();
    }

    public PanelIndicator? SelectedIndicator {
        get;
        set {
            if (SetProperty(ref field, value)) {
                if (value is not null) SelectPanel(value.Index);
                _ = MainThread.InvokeOnMainThreadAsync(() => SelectedIndicator = null);
            }
        }
    }

    [RelayCommand]
    private async Task SwitchActiveProfileAsync() {
        var choices = _profileService.GetProfileNamesWithDefault();
        var index = await ProfileSelector.ShowProfileSelector(choices);
        if (index is {} selectedProfile and >= 0) await _profileService.SwitchProfileByIndexAsync(selectedProfile);
        UpdatePanelIndicators();
    }
    
    [RelayCommand]
    private async Task SwipeLeftAsync() {
        ShowWelcomePage = Panels?.Count <= 0 ? true : false;
        if (Panels?.Any() == true) {
            SelectPanel((CurrentPanelIndex + 1) % Panels.Count);
        }
    }

    [RelayCommand]
    private async Task SwipeRightAsync() {
        ShowWelcomePage = Panels?.Count <= 0 ? true : false;
        if (Panels?.Any() == true) {
            SelectPanel(CurrentPanelIndex <= 0 ? Panels.Count - 1 : CurrentPanelIndex - 1);
        }
    }

    [RelayCommand]
    private async Task SelectPanelAsync(PanelIndicator indicator) {
        SelectPanel(indicator.Index);
    }

    public void SelectPanel(int index) {
        MainThread.BeginInvokeOnMainThread(() => {
            if (index < 0) index = 0;
            if (Panels?.Any() == true && index < Panels.Count) {
               ActivePanel = null;
               CurrentPanelIndex = index;
               ActivePanel = Panels[index];
            }
            ShowWelcomePage = Panels?.Count <= 0 ? true : false;
            UpdatePanelIndicators();
        });
    }

    private void RaiseAllProperties() {
        OnPropertyChanged(nameof(ActivePanel));
        OnPropertyChanged(nameof(ShowWelcomePage));
        OnPropertyChanged(nameof(HideWelcomePage));
        OnPropertyChanged(nameof(HasPanels));
        OnPropertyChanged(nameof(HasNoPanels));
        OnPropertyChanged(nameof(PanelBackgroundColor));
        OnPropertyChanged(nameof(DisplayBackgroundColor));
        OnPropertyChanged(nameof(PanelIndicators));
    }

    public void UpdatePanelIndicators() {
        MainThread.BeginInvokeOnMainThread(() => {
            while (PanelIndicators.Count < Panels?.Count) PanelIndicators.Add(new PanelIndicator(-1, -1));
            while (PanelIndicators.Count > Panels?.Count) PanelIndicators.RemoveAt(PanelIndicators.Count - 1);
            for (var i = 0; i < PanelIndicators.Count; i++) {
                PanelIndicators[i].Index = i;
                PanelIndicators[i].Active = CurrentPanelIndex;
            }
        });
        OnPropertyChanged(nameof(PanelIndicators));
        RaiseAllProperties();        
    }
}

public partial class PanelIndicator(int index, int active) : ObservableObject {
    [ObservableProperty] private int _index = index;
    [ObservableProperty] private int _active = active;
}