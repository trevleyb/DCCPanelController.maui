using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.Services.NavigationService;
using DCCPanelController.View;

namespace DCCPanelController.ViewModel;

public partial class PanelsViewerViewModel : BaseViewModel {
    private readonly SettingsService _settingsService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private int _sidePanelWidth = 300;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelSelected))]
    [NotifyPropertyChangedFor(nameof(NoPanelSelected))]
    [NotifyPropertyChangedFor(nameof(Title))]
    private Panel? _selectedPanel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSidePanelClosed))] 
    [NotifyPropertyChangedFor(nameof(SidePanelWidth))]
    [NotifyPropertyChangedFor(nameof(ShouldShowPanelView))]
    private bool _isSidePanelOpen;
    public bool IsSidePanelClosed => !IsSidePanelOpen;

    public string Title => SelectedPanel == null ? "DCC Panel Controller" : SelectedPanel.Name;
    public bool IsPanelSelected => SelectedPanel != null;
    public bool NoPanelSelected => SelectedPanel == null;
    
    public ObservableCollection<Panel> Panels { get; set; }
    [ObservableProperty] private bool _canExpandCollapse = true;
    
    public bool ShouldShowPanelView => CanExpandCollapse && IsPanelSelected;
    
    public PanelsViewerViewModel() {
        _settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        _navigationService = MauiProgram.ServiceHelper.GetService<NavigationService>();
        Panels = _settingsService.Panels;
        SidePanelWidth = 300;
        IsSidePanelOpen = true;
    }
    
    public async void Save() {
        _settingsService?.Save();
    }

    public async void Load() {
        _settingsService?.Load();
    }

    [RelayCommand]
    private async Task SelectionChangedAsync() {
        Console.WriteLine("Selection Changed");
    }
    
    [RelayCommand]
    private async Task DuplicatePanelAsync(Panel? panel = null) {
        if (panel is not null) SelectedPanel = panel;
        if (SelectedPanel is null) return;
        var newPanel = SelectedPanel.Clone();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        newPanel.Name = "Panel " + maxSort;
        newPanel.SortOrder = maxSort;
        Panels.Add(newPanel);
        Save();
    }

    [RelayCommand]
    private async Task AddPanelAsync() {
        var panel = new Panel();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        panel.Name = "Panel " + maxSort;
        panel.SortOrder = maxSort;
        Panels.Add(panel);
        Save();
    }

    [RelayCommand]
    private async Task EditPanelAsync(Panel? panel = null) {
        if (panel is not null) SelectedPanel = panel;
        if (SelectedPanel is null) return;
        try {
            var result = await _navigationService.NavigateToPanelEditor(SelectedPanel);
            if (result is not null) Save();
        } catch (Exception ex) {
            Console.WriteLine($"Failed to goto the Panel details for {SelectedPanel.Name} due to {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeletePanelAsync(Panel? panel = null) {
        if (panel is not null) SelectedPanel = panel;
        if (SelectedPanel is null) return;
        try {
            Panels.Remove(SelectedPanel);
            for (var index = 0; index < Panels.Count; index++) {
                Panels[index].SortOrder = index + 1;
            }
            Save();
        } catch {
            Console.WriteLine($"Failed to delete panel {SelectedPanel.Name}");
        }
    }

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex MyRegex();
}