using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.View;

namespace DCCPanelController.ViewModel;

public partial class PanelsViewModel : BaseViewModel {
    
    public ObservableCollection<Panel> Panels { get; } = [];
    private readonly SettingsService _settingsService;
    private readonly PanelsPage _sender;

    public PanelsViewModel(SettingsService settingsService, PanelsPage sender) {
        Title = "Panels List";
        _settingsService = settingsService;
        _sender = sender;
    }

    [RelayCommand]
    public async Task AddNewPanelAsync() {
        var panel = new Panel();
        panel.Id = Panels.Count == 0 ? "1" : Panels.Max(p => GetNumericPart(p.Id) + 1)?.ToString() ?? "1";
        panel.Name = "Panel " + panel.Id;
        Panels.Add(panel);
        await _sender.Navigation.PushAsync(new PanelDetailsPage(panel));
    }

    private static string GetNumericPart(string input) => Regex.Replace(input, @"[^0-9]", "");
    
    [RelayCommand]
    public async Task GoToDetailsAsync(Panel panel) {
        Console.WriteLine(panel.Name);
        await _sender.Navigation.PushAsync(new PanelDetailsPage(panel));
    }

    [RelayCommand]
    public async Task GetPanelsAsync()
    {
        if (Panels.Count > 0) {
            IsBusy = false;
            IsRefreshing = false;
        }
        
        if (IsBusy) return;
        try {
            IsBusy = true;
            var panels = _settingsService.Panels;
            Panels.Clear();
            foreach(var panel in panels) Panels.Add(panel);
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
}