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
        Title = "Panels List";
        _settingsService = settingsService;
        _sender = sender;
        GetPanelsCommand.Execute(null);
    }
    
    [RelayCommand]
    public async Task AddNewPanelAsync() {
        var panel = new Panel();
        var maxSort = Panels.Max(p => p.SortOrder) + 1;
        
        panel.Id = "new" + maxSort;
        panel.Name = "Panel " + maxSort;
        panel.SortOrder = maxSort;
        Panels.Add(panel);
        //await _sender.Navigation.PushAsync(new PanelDetailsPage(panel));
    }

    private static int GetNumericPart(string input) => Convert.ToInt32(MyRegex().Replace(input, ""));
    
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

    [RelayCommand]
    public async Task GetPanelsAsync()
    {
        if (Panels.Count > 0 || IsBusy) {
            IsBusy = false;
            IsRefreshing = false;
            return;
        }
        
        try {
            IsBusy = true;
            var panels = _settingsService.Panels;
            var sortOrder = 1;
            Panels.Clear();
            foreach (var panel in panels.OrderBy(p => p.SortOrder)) {
                panel.SortOrder = sortOrder++;
                Panels.Add(panel);
            }
        }
        catch (Exception ex) {
            Debug.WriteLine($"Unable to get Panels: {ex.Message}");
            await Shell.Current.DisplayAlert("Error! Cannot get Panels States", ex.Message, "OK");
        }
        finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex MyRegex();
}