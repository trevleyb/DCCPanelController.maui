using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;
using DCCPanelController.View.Properties;

namespace DCCPanelController.View;

public partial class TurnoutsViewModel : ConnectionViewModel {
    private bool _isAscending;
    private string _sortColumn = "";
    
    private const string LabelID = "ID";
    private const string LabelName = "Turnout";
    private const string LabelState = "State";
    private const string LabelAddress = "DCC Address";
    
    [ObservableProperty] private string _columnLabelAddress = LabelAddress;
    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;
    [ObservableProperty] private ObservableCollection<Turnout> _turnouts;

    public double ScreenWidth = 100;
    public double ScreenHeight = 100;
    public INavigation? Navigation;
    
    public TurnoutsViewModel(Profile profile, ConnectionService connectionService) : base(profile,connectionService) {
        Turnouts = Profile.Turnouts;
        SetLabels();
    }
    
    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals("State") ? _isAscending.GetSortDirection() : "");
        ColumnLabelAddress = LabelAddress + (_sortColumn.Equals("DCCAddress") ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    private async Task RefreshTurnoutsAsync() {
        try {
            IsBusy = true;
            Profile.Turnouts.Clear();
            await ConnectionService.ForceRefresh();
        } catch (Exception ex){
            Console.WriteLine($"Unable to force refresh the turnouts: {ex.Message}");
        } finally {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Turnout> sortedTurnout;

        if (!_isAscending) {
            sortedTurnout = columnName.ToLower() switch {
                "name"       => Turnouts.OrderBy<Turnout, string>(x => x.Name ?? "").ToList(),
                "dccaddress" => Turnouts.OrderBy<Turnout, string>(x => x.DccAddress ?? "").ToList(),
                "id"         => Turnouts.OrderBy<Turnout, string>(x => x.Id ?? "").ToList(),
                "state"      => Turnouts.OrderBy<Turnout, TurnoutStateEnum>(x => x.State).ToList(),
                _            => Turnouts.ToList<Turnout>()
            };
        } else {
            sortedTurnout = columnName.ToLower() switch {
                "name"       => Turnouts.OrderByDescending<Turnout, string>(x => x.Name ?? "").ToList(),
                "id"         => Turnouts.OrderByDescending<Turnout, string>(x => x.Id ?? "").ToList(),
                "dccaddress" => Turnouts.OrderByDescending<Turnout, string>(x => x.DccAddress ?? "").ToList(),
                "state"      => Turnouts.OrderByDescending<Turnout, TurnoutStateEnum>(x => x.State).ToList(),
                _            => Turnouts.ToList<Turnout>()
            };
        }

        _sortColumn = columnName;
        _isAscending = !_isAscending;

        Turnouts = new ObservableCollection<Turnout>(sortedTurnout);
        OnPropertyChanged(nameof(Turnouts));
        SetLabels();
    }

    [RelayCommand]
    private async Task DeleteTurnoutAsync(Turnout? turnout) {
        if (turnout is not null) {
            Turnouts.Remove(turnout);
            OnPropertyChanged(nameof(Turnouts));
            await Profile.SaveAsync();
        }
    }

    [RelayCommand]
    private async Task AddTurnoutAsync() {
        var turnout = new Turnout {
            Id = TurnoutAnalyzer.GetUniqueID(Turnouts.ToList<Turnout>()),
            Name = "New Turnout",
            State = TurnoutStateEnum.Closed,
            Default = TurnoutStateEnum.Closed,
            IsEditable = true
        };
        Turnouts.Add(turnout);
        await EditTurnoutAsync(turnout);
        await Profile.SaveAsync();
        OnPropertyChanged(nameof(Turnouts));
    }

    [RelayCommand]
    private async Task SendTurnoutStateAsync(Turnout? turnout) {
        if (turnout is not null) {
            if (!string.IsNullOrEmpty(turnout.DccAddress) && IsConnected) {
                await ConnectionService?.SendTurnoutCmdAsync(turnout.Name ?? "", turnout.State == TurnoutStateEnum.Thrown)!;
            }
            OnPropertyChanged(nameof(Turnouts));
        }
    }
    
    [RelayCommand]
    public async Task EditTurnoutAsync(Turnout? turnout) {
        try {
            if (turnout is not null && Navigation is {  } navigation) {
                var turnoutsEditViewModel = new TurnoutsEditViewModel(turnout, ConnectionService);
                await PropertyDisplayService.ShowPropertiesAsync (navigation, turnoutsEditViewModel, ScreenWidth, ScreenHeight);
                await Profile.SaveAsync();
                OnPropertyChanged(nameof(Turnouts));
            }
        } catch (Exception ex) {
            Console.WriteLine("Error Launching Panel Properties Page: " + ex.Message);
        }
    }
    
    [RelayCommand]
    private async Task ClearAllAsync() {
        IsBusy = true;
        try {
            if (await AskUserToConfirm("Reset all Turnouts?", "This wll remove all Turnouts previously loaded from a Server (leaving manually added Turnouts) and reload them from the Connected Server. Are you sure you want to do this?")) {
                var removeTurnouts = Profile.Turnouts.Where(turnout => turnout.IsEditable == false).ToList();
                foreach (var turnout in removeTurnouts) {
                    Profile.Turnouts.Remove(turnout);
                    OnPropertyChanged(nameof(Turnouts));
                }
                await RefreshTurnoutsAsync();
            }
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }
}