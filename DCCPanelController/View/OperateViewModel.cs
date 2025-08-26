using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DCCPanelController.View;

public partial class OperateViewModel : Base.ConnectionViewModel {
    [ObservableProperty] private bool _showGrid;
    [ObservableProperty] private bool _showPath;
    [ObservableProperty] private bool _isMaximized;
    [ObservableProperty] private int _currentPanelIndex;
    [ObservableProperty] private Panel? _activePanel;

    [NotifyPropertyChangedFor(nameof(HideWelcomePage))]
    [ObservableProperty] private bool _showWelcomePage;
    public bool HideWelcomePage => !ShowWelcomePage;

    public string IconMinimize => "minimize_2";
    public string IconMaximize => "maximize_2";
    
    public Color BackgroundColor => ActivePanel?.DisplayBackgroundColor ?? Colors.White;
    public ObservableCollection<Panel> Panels { get; private set; }
    public string ProfileName => _profileService?.ActiveProfile?.ProfileName ?? "No Profile";
    
    public bool HasPanels => Panels?.Any() == true;
    public bool HasNoPanels => !HasPanels;
    
    public ObservableCollection<PanelIndicator> PanelIndicators { get; private set; } = [];

    private ILogger<OperateViewModel> _logger;
    private readonly ProfileService _profileService;
    
    public OperateViewModel(ILogger<OperateViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        Panels = [];  
        CurrentPanelIndex = -1;
    }

    public async Task LoadPanelsAsync() {
        try {
            MainThread.BeginInvokeOnMainThread(() => {
                var profile = _profileService.ActiveProfile;
                Panels = profile?.Panels ?? throw new ApplicationException($"OperateViewModel: Panels Collection should not be empty.");
                ShowWelcomePage = Panels.Count <= 0 || (profile?.Settings?.ShowWelcomePage ?? true);
            
                if (!ShowWelcomePage) SelectPanel(0);
                UpdatePanelIndicators();
                
                OnPropertyChanged(nameof(HasPanels));
                OnPropertyChanged(nameof(HasNoPanels));
                OnPropertyChanged(nameof(PanelIndicators));
                OnPropertyChanged(nameof(ShowWelcomePage));
            });
        } catch (Exception ex) {
            _logger.LogError("Error loading panels: {ExMessage}", ex.Message);
        }
    }
    
    [RelayCommand]
    private void SwipeLeft() {
        ShowWelcomePage = Panels?.Count <= 0 ? true : false;
        if (Panels?.Any() == true) {
            SelectPanel((CurrentPanelIndex + 1) % Panels.Count);
        }
    }

    [RelayCommand]
    private void SwipeRight() {
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
        if (index < 0) index = 0;
        if (Panels?.Any() == true && index < Panels.Count) {
            ActivePanel = null;
            CurrentPanelIndex = index;
            ActivePanel = Panels[index];
            ShowWelcomePage = false;
        }
        OnPropertyChanged(nameof(ActivePanel));
        OnPropertyChanged(nameof(ShowWelcomePage));
        OnPropertyChanged(nameof(HideWelcomePage));
        OnPropertyChanged(nameof(HasPanels));
        OnPropertyChanged(nameof(HasNoPanels));
        OnPropertyChanged(nameof(BackgroundColor));
        UpdatePanelIndicators();
        ShowWelcomePage = Panels?.Count <= 0 ? true : false;
    }

    public void UpdatePanelIndicators() {
        while (PanelIndicators.Count < Panels?.Count) PanelIndicators.Add(new PanelIndicator(-1,-1));
        while (PanelIndicators.Count > Panels?.Count) PanelIndicators.RemoveAt(PanelIndicators.Count - 1);
        for (int i = 0; i < PanelIndicators.Count; i++) {
            PanelIndicators[i].Index = i;
            PanelIndicators[i].Active = CurrentPanelIndex;
        }
        OnPropertyChanged(nameof(PanelIndicators));
    }
}

public partial class PanelIndicator(int index, int active) : ObservableObject {
    [ObservableProperty] private int _index = index;
    [ObservableProperty] private int _active = active;
    public bool IsActive => Active == Index; 
}