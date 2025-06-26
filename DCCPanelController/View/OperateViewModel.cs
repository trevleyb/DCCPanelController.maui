using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class OperateViewModel : Base.ConnectionViewModel {
    [ObservableProperty] private bool _showGrid;
    [ObservableProperty] private bool _showPath;
    [ObservableProperty] private bool _showWelcomePage;
    [ObservableProperty] private int _currentPanelIndex;
    [ObservableProperty] private Panel? _activePanel;

    public Color BackgroundColor => ActivePanel?.DisplayBackgroundColor ?? Colors.White;
    public ObservableCollection<Panel> Panels { get; set; } = [];

    public bool HideWelcomePage => !ShowWelcomePage;
    public bool HasPanels => Panels?.Any() == true || ShowWelcomePage;
    public bool HasNoPanels => !HasPanels;
    public List<int> PanelIndicators => Enumerable.Range(0, Panels?.Count ?? 0).ToList();

    private readonly ProfileService _profileService;
    public OperateViewModel(ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _profileService = profileService;
        _ = Task.Run(async () => await LoadPanelsAsync());
    }

    private async Task LoadPanelsAsync() {
        try {
            var profile = await _profileService.GetActiveProfileAsync();
            var panels = profile?.Panels?.ToList() ?? [];
            ShowWelcomePage = panels.Count <= 0 || (profile?.Settings?.ShowWelcomePage ?? true);
            
            MainThread.BeginInvokeOnMainThread(() => {
                Panels.Clear();
                foreach (var panel in panels) Panels.Add(panel);

                CurrentPanelIndex = -1;
                if (!ShowWelcomePage) SelectPanel(0);
                
                OnPropertyChanged(nameof(HasPanels));
                OnPropertyChanged(nameof(HasNoPanels));
                OnPropertyChanged(nameof(PanelIndicators));
            });
        } catch (Exception ex) {
            Console.WriteLine($"Error loading panels: {ex.Message}");
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
        if (ShowWelcomePage) index = 0;
        if (Panels?.Any() == true && index >= 0 && index < Panels.Count) {
            ActivePanel = null;
            CurrentPanelIndex = index;
            ActivePanel = Panels[index];
        }
        OnPropertyChanged(nameof(ActivePanel));
        OnPropertyChanged(nameof(ShowWelcomePage));
        OnPropertyChanged(nameof(HideWelcomePage));
        OnPropertyChanged(nameof(PanelIndicators));
        ShowWelcomePage = false;
    }
}