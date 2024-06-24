using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class RoutesViewModel : BaseViewModel {
    
    public ObservableCollection<Route> Routes { get; } = [];
    private readonly RoutesService? _routesService;
 
    public RoutesViewModel(RoutesService? routesService) {
        Title = "Active Routes";
        _routesService = routesService;
    }

    [RelayCommand]
    public async Task GetRoutesDataAsync()
    {
        if (IsBusy || _routesService == null) return;
        try {
            IsBusy = true;
            var routes = await _routesService.GetRoutes();
            if (Routes?.Count != 0) Routes?.Clear();
            foreach(var route in routes) Routes?.Add(route);
        }
        catch (Exception ex) {
            Debug.WriteLine($"Unable to get Routes: {ex.Message}");
            await Shell.Current.DisplayAlert("Error! Cannot get Routes", ex.Message, "OK");
        }
        finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    
    [RelayCommand]
    public async Task ToggleRoutesState(Route? route) {
        
        if (route == null) return;
        route.State = route.State switch {
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            RouteStateEnum.Inactive => RouteStateEnum.Active,
            _                       => RouteStateEnum.Active
        };
    }

}