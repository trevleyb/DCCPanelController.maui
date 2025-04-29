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
        if (ConnectionService.IsConnected) {
            Client = await ConnectionService.Connect(Profile.ActiveConnectionInfo);
            Client?.Disconnect();
        }
        Client = await ConnectionService.Connect(Profile.ActiveConnectionInfo);
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
            Client = await ConnectionService.Connect(Profile.ActiveConnectionInfo);
            Client?.SendTurnoutCmd(turnout.DccAddress ?? "", turnout.State == TurnoutStateEnum.Thrown);
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
}