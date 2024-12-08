using System.Collections.ObjectModel;
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

    public PanelsViewModel() {
        _settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        _navigationService = MauiProgram.ServiceHelper.GetService<NavigationService>();
        Panels = _settingsService.Panels;
    }

    public ObservableCollection<Panel> Panels { get; set; }

    public async void Save() {
        _settingsService?.Save();
    }

    public async void Load() {
        _settingsService?.Load();
    }

    [RelayCommand]
    public async Task AddNewPanelAsync() {
        var panel = new Panel();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        panel.Name = "Panel " + maxSort;
        panel.SortOrder = maxSort;
        Panels.Add(panel);
    }

    [RelayCommand]
    public async Task EditPanelAsync(Panel panel) {
        Console.WriteLine($"Stopping here to check out the panel {panel.Name}");
        try {
            Console.WriteLine($"Launch Editor Selected Panel: {panel.Name}");
            await _navigationService.NavigateToPanelEditor(panel);
        } catch (Exception ex) {
            Console.WriteLine($"Failed to goto the Panel details for {panel.Name} due to {ex.Message}");
        }
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