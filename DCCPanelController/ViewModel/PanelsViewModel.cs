using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.View;

namespace DCCPanelController.ViewModel;

public partial class PanelsViewModel : BaseViewModel {
    
    public ObservableCollection<Panel> Panels { get; } = [];
    private readonly PanelsService _panelsService;

    public PanelsViewModel(PanelsService panelsService) {
        Title = "Panels List";
        _panelsService = panelsService;
    }
    
    [RelayCommand]
    async Task GoToDetails(Panel panel) {
        if (panel == null) return;

        await Shell.Current.GoToAsync(nameof(PanelDetailsPage), true, new Dictionary<string, object>
        {
            {"Panel", panel }
        });
    }
    
    [RelayCommand]
    public async Task GetPanelsAsync()
    {
        if (IsBusy) return;
        try {
            IsBusy = true;
            var panels = await _panelsService.GetPanels();
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