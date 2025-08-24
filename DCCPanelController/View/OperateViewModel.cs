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
    [ObservableProperty] private bool _showWelcomePage;
    [ObservableProperty] private int _currentPanelIndex;
    [ObservableProperty] private Panel? _activePanel;
    [ObservableProperty] private bool _isMaximized;

    public string IconMinimize => "minimize_2";
    public string IconMaximize => "maximize_2";
    
    public Color BackgroundColor => ActivePanel?.DisplayBackgroundColor ?? Colors.White;
    public ObservableCollection<Panel> Panels { get; private set; }

    public bool HideWelcomePage => !ShowWelcomePage;
    public bool HasPanels => Panels?.Any() == true || ShowWelcomePage;
    public bool HasNoPanels => !HasPanels;
    public ObservableCollection<int> PanelIndicators { get; private set; } = [];

    private ILogger<OperateViewModel> _logger;
    private readonly ProfileService _profileService;
    public OperateViewModel(ILogger<OperateViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        Panels = [];        // Just set it to nothing until it is re-assigned. 
        _ = Task.Run(async () => await LoadPanelsAsync());
    }
    
    private async Task LoadPanelsAsync() {
        try {
            var profile = _profileService.ActiveProfile;
            Panels = profile?.Panels ?? throw new ApplicationException($"OperateViewModel: Panels Collection should not be empty.");
            ShowWelcomePage = Panels.Count <= 0 || (profile?.Settings?.ShowWelcomePage ?? true);
            
            MainThread.BeginInvokeOnMainThread(() => {
                UpdatePanelIndicators();
                
                CurrentPanelIndex = -1;
                if (!ShowWelcomePage) SelectPanel(0);
                
                OnPropertyChanged(nameof(HasPanels));
                OnPropertyChanged(nameof(HasNoPanels));
                OnPropertyChanged(nameof(PanelIndicators));
            });
        } catch (Exception ex) {
            _logger.LogError("Error loading panels: {ExMessage}", ex.Message);
        }
    }

    public void UpdatePanelIndicators() {
        PanelIndicators.Clear();
        for (var i = 0; i < (Panels?.Count ?? 0); i++) {
            PanelIndicators.Add(i);
        }
    }

    
    [RelayCommand]
    private void SwipeLeft() {
        if (Panels?.Any() == true) {
            SelectPanel((CurrentPanelIndex + 1) % Panels.Count);
        }
    }

    [RelayCommand]
    private void SwipeRight() {
        if (Panels?.Any() == true) {
            SelectPanel(CurrentPanelIndex <= 0 ? Panels.Count - 1 : CurrentPanelIndex - 1);
        }
    }

    [RelayCommand]
    private void SelectPanel(int index) {
        if (index < 0) index = 0;
        if (Panels?.Any() == true && index < Panels.Count) {
            ActivePanel = null;
            CurrentPanelIndex = index;
            ActivePanel = Panels[index];
        }
        OnPropertyChanged(nameof(ActivePanel));
        OnPropertyChanged(nameof(ShowWelcomePage));
        OnPropertyChanged(nameof(HideWelcomePage));
        UpdatePanelIndicators();
        ShowWelcomePage = false;
    }
}