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
    private int _draggingIndex;
    
    private const int minCardHeight = 200;
    
    public PanelsViewModel(SettingsService settingsService) {
        _settingsService = settingsService;
        Panels = settingsService.Panels;
    }
    
    public PanelsPage? Sender { get; set; }
    
    public int CardHeight { get; set; } = minCardHeight;

    public void SetCardHeight(double width, double height) {
        var imageWidth = Panels.Average(x => x.OriginalImageWidth);
        var imageHeight = Panels.Average(x => x.OriginalImageHeight);
        var itemHeight = (int)(height / CalculateItemsToShow (imageWidth, imageHeight));
        CardHeight = (int)(itemHeight > minCardHeight ? itemHeight : minCardHeight);
        foreach (var item in Panels) item.CardHeight = CardHeight;
    }

    public int CalculateItemsToShow (double width, double height) {
        // What is the proportion of the Height to the width of the current Image. 
        // Based on this, determine if we should have 2, 3, or 4 items visible
        if (height <= 0 || width <= 0) return 2;

        var ratio = 2;
        ratio = height > width ? Math.Max((int)(height / width), 2) : Math.Max((int)(width / height), 2);
        return ratio;
    }

    [RelayCommand]
    public async Task AddNewPanelAsync() {
        var panel = new Panel();
        var maxSort = Panels.Count > 0 ? Panels.Max(p => p.SortOrder) + 1 : 1;
        
        panel.Id = "new" + maxSort;
        panel.Name = "Panel " + maxSort;
        panel.SortOrder = maxSort;
        panel.CardHeight = CardHeight;
        Panels.Add(panel);
        
        await _settingsService.Save();
    }

    [RelayCommand]
    public async Task DeletePanelAsync(Panel panel) {
        Panels.Remove(panel);
        for (int index = 0; index < Panels.Count; index++) {
            Panels[index].SortOrder = index+1;
        }
        await _settingsService.Save();
    }

    [RelayCommand]
    public async Task GoToDetailsAsync(Panel panel) {
        if (Sender != null) {
            await Sender.Navigation.PushAsync(new PanelDetailsPage(panel));
        }
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