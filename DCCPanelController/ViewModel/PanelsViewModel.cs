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

public partial class PanelsViewModel : BaseViewModel {
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
    private bool _isSidePanelOpen;
    public bool IsSidePanelClosed => !IsSidePanelOpen;

    public string Title => SelectedPanel == null ? "DCC Panel Controller" : SelectedPanel.Name;
    public bool IsPanelSelected => SelectedPanel != null;
    public bool NoPanelSelected => SelectedPanel == null;
    
    public ObservableCollection<Panel> Panels { get; set; }

    public PanelsViewModel() {
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
    private async Task SelectionChangedAsync() { }
    
    [RelayCommand]
    public async Task AddPanelAsync() {
        var panel = new Panel();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        panel.Name = "Panel " + maxSort;
        panel.SortOrder = maxSort;
        Panels.Add(panel);
        Save();
    }

    [RelayCommand]
    public async Task EditPanelAsync(Panel panel) {
        // Console.WriteLine($"Stopping here to check out the panel {panel.Name}");
        // try {
        //     Console.WriteLine($"Launch Editor Selected Panel: {panel.Name}");
        //     var result = await _navigationService.NavigateToPanelEditor(panel);
        //     if (result is not null) {
        //         Console.WriteLine($"Result from Editor: {result.Name}");
        //         Save();
        //     } else {
        //         Console.WriteLine($"Result from Editor: {result?.Name ?? "null"}");
        //     }
        // } catch (Exception ex) {
        //     Console.WriteLine($"Failed to goto the Panel details for {panel.Name} due to {ex.Message}");
        // }
    }

    [RelayCommand]
    public async Task DeletePanelAsync(Panel panel) {
        try {
            Panels.Remove(panel);
            for (var index = 0; index < Panels.Count; index++) {
                Panels[index].SortOrder = index + 1;
            }
            Save();
        } catch {
            Console.WriteLine($"Failed to delete panel {panel.Name}");
        }
    }

    public void OnEditorPageFinished(Panel panel) {
        // What we need to do is force the Panel/Card that we are associated with
        // to refresh. It should be doing this as the SystemName/ID are refreshing, but not
        // the PanelViewer.
        Panels[Panels.IndexOf(panel)] = panel;
    }

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex MyRegex();
}