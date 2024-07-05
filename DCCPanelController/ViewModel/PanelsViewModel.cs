using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.View;

namespace DCCPanelController.ViewModel;

public partial class PanelsViewModel : BaseViewModel {
    
    public ObservableCollection<Panel> Panels { get; set; } = [];
    private readonly SettingsService _settingsService;
    private readonly PanelsPage _sender;
    private int _draggingIndex;
    
    public PanelsViewModel(SettingsService settingsService, PanelsPage sender) {
        _settingsService = settingsService;
        _sender = sender;
        Panels = settingsService.Panels;
    }
    
    [RelayCommand]
    public async Task AddNewPanelAsync() {
        var panel = new Panel();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        
        panel.Id = "new" + maxSort;
        panel.Name = "Panel " + maxSort;
        panel.SortOrder = maxSort;
        Panels.Add(panel);
        
        _settingsService.Save();
    }

    [RelayCommand]
    public async Task GoToDetailsAsync(Panel panel) {
        Console.WriteLine(panel.Name);
        await _sender.Navigation.PushAsync(new PanelDetailsPage(panel));
    }

    [RelayCommand]
    public async Task DragAsync(Panel panel) { 
        _draggingIndex = Panels.IndexOf(panel);
    }

    [RelayCommand]
    public async Task DropAsync(Panel panel) {
        int droppedIndex = Panels.IndexOf(panel);

        // Swap or rearrange items
        if (_draggingIndex >= 0 && droppedIndex >= 0) {
            var draggedItem = Panels[_draggingIndex];
            Panels.Remove(draggedItem);
            Panels.Insert(droppedIndex, draggedItem);

            // ReApply the Sort Order so we order the list by this number
            // ------------------------------------------------------------
            for (int index = 0; index < Panels.Count; index++) {
                Panels[index].SortOrder = index+1;
            }
        }
    } 

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex MyRegex();
}