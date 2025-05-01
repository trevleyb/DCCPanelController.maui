using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClients;
using DCCClients.Jmri.JMRI.DataBlocks;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class TurnoutsViewModel : BaseViewModel {
    private const string LabelID = "ID";
    private const string LabelName = "Turnout";
    private const string LabelState = "State";
    private const string LabelAddress = "DCC Address";

    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _columnLabelAddress = LabelAddress;
    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;

    private bool _isAscending;
    private string _sortColumn = "";
    [ObservableProperty] private ObservableCollection<Turnout> _turnouts;

    public TurnoutsViewModel(Profile profile, ConnectionService connectionService) {
        Profile = profile;
        Turnouts = Profile.Turnouts;
        ConnectionService = connectionService;
        ConnectionService.ConnectionChanged += (sender, args) => {
            IsConnected = args.IsConnected;
        };

        SetLabels();
    }

    private ConnectionService ConnectionService { get; }
    private IDccClient? Client { get; set; }
    private Profile Profile { get; }

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
            var result = await ConnectionService.Connect(Profile.ActiveConnectionInfo);
            if (result.IsSuccess) {
                Client = result.Value;
                Client?.ForceRefresh();
            } else {
                Client = null;
            }
        } catch { /* ignore */
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
    private async Task EditTurnoutAsync(Turnout? turnout) {
        await NavigateToEditTurnoutAsync(turnout);
        OnPropertyChanged(nameof(Turnouts));
        Profile.Save();
    }

    [RelayCommand]
    private async Task DeleteTurnoutAsync(Turnout Turnout) {
        Turnouts.Remove(Turnout);
        OnPropertyChanged(nameof(Turnouts));
        Profile.Save();
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

        var result = await NavigateToEditTurnoutAsync(turnout);
        if (result is not null) Turnouts.Add(result);
        OnPropertyChanged(nameof(Turnouts));
        Profile.Save();
    }

    [RelayCommand]
    private async Task SendTurnoutStateAsync(Turnout? turnout) {
        if (turnout == null) return;
        if (!string.IsNullOrEmpty(turnout.DccAddress)) {
            var result = await ConnectionService.Connect(Profile.ActiveConnectionInfo);
            if (result.IsSuccess) {
                Client?.SendTurnoutCmd(turnout.DccAddress ?? "", turnout.State == TurnoutStateEnum.Thrown);
            } 
        }
        OnPropertyChanged(nameof(Turnouts));
    }

    [RelayCommand]
    private async Task ToggleTurnoutStateAsync(Turnout? turnout) {
        if (turnout == null) return;
        turnout.State = turnout.State switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed
        };

        await SendTurnoutStateAsync(turnout);
        OnPropertyChanged(nameof(Turnouts));
    }

    public async Task<Turnout?> NavigateToEditTurnoutAsync(Turnout? turnout) {
        if (turnout is null) return null;

        var mainPage = App.Current.Windows[0].Page;
        if (mainPage == null) throw new InvalidOperationException("MainPage is not set.");

        var editPage = new TurnoutsEditView(turnout, ConnectionService);
        var tcs = new TaskCompletionSource<Turnout?>();

        if (editPage.ViewModel != null) {
            editPage.ViewModel.OnSaveCompleted += turnoutResult => {
                tcs.SetResult(turnoutResult);
                mainPage.Navigation.PopModalAsync();
            };
        }

        await mainPage.Navigation.PushModalAsync(editPage);
        return await tcs.Task;
    }

    [RelayCommand]
    private async Task ClearAllAsync() {
        IsBusy = true;
        try {
            if (await AskUserToConfirm("Reset all Turnouts?", "This wll remove all Tunrouts previously loaded from a Server (leaving manually added Turnouts) and reload them from the Connected Server. Are you sure you want to do this?")) {
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
    
    private async Task<bool> AskUserToConfirm(string title, string message) {
        if (App.Current.Windows[0].Page is { } window) {
            var result = await window.DisplayAlert(
                title,
                message,
                "Yes",
                "No"
            );
            return result;
        }
        return false;
    }

    
    [RelayCommand]
    private async Task ToggleConnectionAsync() {
        if (!IsConnected) {
            await ConnectionService.Connect();
        } else {
            ConnectionService.Disconnect();
        }
    }

}