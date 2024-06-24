using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Services;

namespace DCCPanelController.ViewModel;

public partial class TurnoutStateViewModel : BaseViewModel {

    public ObservableCollection<TurnoutState> TurnoutStates { get; } = [];
    private readonly TurnoutStateService? _turnoutStateService;
 
    public TurnoutStateViewModel(TurnoutStateService? turnoutStateService) {
        Title = "Active Turnouts";
        _turnoutStateService = turnoutStateService;
    }

    [RelayCommand]
    public async Task GetTurnoutStateDataAsync()
    {
        if (IsBusy || _turnoutStateService == null) return;
        try {
            IsBusy = true;
            var turnoutStates = await _turnoutStateService.GetTurnoutStates();
            if (TurnoutStates?.Count != 0) TurnoutStates?.Clear();
            foreach(var turnout in turnoutStates) TurnoutStates?.Add(turnout);
        }
        catch (Exception ex) {
            Debug.WriteLine($"Unable to get Turnout States: {ex.Message}");
            await Shell.Current.DisplayAlert("Error! Cannot get Turnout States", ex.Message, "OK");
        }
        finally {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task ToggleTurnoutState(TurnoutState? turnoutState) {
        
        if (turnoutState == null) return;
        turnoutState.State = turnoutState.State switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed
        };
    }
}

/*
  
    [RelayCommand]
    async Task GoToDetails(Monkey monkey)
    {
        if (monkey == null)
        return;

        await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
        {
            {"Monkey", monkey }
        });
    }

    [ObservableProperty]
    bool isRefreshing;

    [RelayCommand]
    async Task GetMonkeysAsync()
    {
        if (IsBusy)
            return;

        try
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("No connectivity!",
                    $"Please check internet and try again.", "OK");
                return;
            }

            IsBusy = true;
            var monkeys = await monkeyService.GetMonkeys();

            if(Monkeys.Count != 0)
                Monkeys.Clear();

            foreach(var monkey in monkeys)
                Monkeys.Add(monkey);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get monkeys: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }

    }

    [RelayCommand]
    async Task GetClosestMonkey()
    {
        if (IsBusy || Monkeys.Count == 0)
            return;

        try
        {
            // Get cached location, else get real location.
            var location = await geolocation.GetLastKnownLocationAsync();
            if (location == null)
            {
                location = await geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(30)
                });
            }

            // Find closest monkey to us
            var first = Monkeys.OrderBy(m => location.CalculateDistance(
                new Location(m.Latitude, m.Longitude), DistanceUnits.Miles))
                .FirstOrDefault();

            await Shell.Current.DisplayAlert("", first.Name + " " +
                first.Location, "OK");

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to query location: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }
}
*/