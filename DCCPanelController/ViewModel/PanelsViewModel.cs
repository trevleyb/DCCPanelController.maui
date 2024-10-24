using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.View;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.ViewModel;

public partial class PanelsViewModel : BaseViewModel {

    [ObservableProperty] private Panel? _selectedPanel = null;
    [ObservableProperty] private ObservableCollection<Panel> _panels;

    private readonly SettingsService _settingsService;
    private int _draggingIndex;
    
    public PanelsViewModel() {
        _settingsService = MauiProgram.ServiceHelper.GetService<SettingsService>();
        Panels = _settingsService.Panels;
    }
    
    public async void Save() => _settingsService?.Save();
    public async void Load() => _settingsService?.Load();
    
    [RelayCommand]
    public async Task AddNewPanelAsync() {
        var panel = new Panel();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        panel.Name = "Panel " + maxSort;
        panel.SortOrder = maxSort;
        Panels.Add(panel);
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
        // to refresh. It should be doing this as the Name/ID are refreshing, but not
        // the PanelViewer.
        Panels[Panels.IndexOf(panel)] = panel;
    }

    [RelayCommand]
    public async Task DragAsync(Panel panel) { 
        _draggingIndex = Panels.IndexOf(panel);
    }

    [RelayCommand]
    public async Task DropAsync(Panel panel) {
        var droppedIndex = Panels.IndexOf(panel);
        // Swap or rearrange items
        if (_draggingIndex >= 0 && droppedIndex >= 0) {
            var draggedItem = Panels[_draggingIndex];
            Panels.Remove(draggedItem);
            Panels.Insert(droppedIndex, draggedItem);

            // ReApply the Sort Order so we order the list by this number
            // ------------------------------------------------------------
            for (var index = 0; index < Panels.Count; index++) {
                Panels[index].SortOrder = index+1;
            }
        }
    } 

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex MyRegex();
}