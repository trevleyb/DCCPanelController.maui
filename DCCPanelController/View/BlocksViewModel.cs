using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClients;
using DCCClients.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using RouteStateEnum = DCCPanelController.Models.DataModel.Entities.RouteStateEnum;

namespace DCCPanelController.View;

public partial class BlocksViewModel : BaseViewModel {
    private const string LabelID = "ID";
    private const string LabelName = "Block";
    private const string LabelState = "Is Occupied?";

    [ObservableProperty] private bool _canToggleRoutesState;
    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;

    private bool _isAscending;
    [ObservableProperty] private ObservableCollection<Block> _blocks;
    private string _sortColumn = "";

    public BlocksViewModel(Profile profile, ConnectionService connectionService) {
        ConnectionService = connectionService;
        Profile = profile;
        Blocks = Profile.Blocks;
        CanToggleRoutesState = true;
        SetLabels();
    }

    private Profile Profile { get; }
    private IDccClient? Client { get; set; }
    private ConnectionService ConnectionService { get; }

    private void ClientOnRouteMsgReceived(object? sender, DccOccupancyArgs e) {
        if (Blocks.Any(x => x.Id == e.BlockId)) {
            var block = Blocks.First(x => x.Id == e.BlockId);
            block.IsOccupied = e.IsOccupied;
        } else {
            Blocks.Add(new Block { Id = e.BlockId, IsOccupied = e.IsOccupied});
        }
    }

    [RelayCommand]
    public async Task SortByColumn(string columnName) {
        List<Block> sortedBlocks;

        if (!_isAscending) {
            sortedBlocks = columnName.ToLower() switch {
                "name"  => Blocks.OrderBy<Block, string>(x => x.Name ?? "").ToList(),
                "id"    => Blocks.OrderBy<Block, string>(x => x.Id ?? "").ToList(),
                "state" => Blocks.OrderBy<Block, bool>(x => x.IsOccupied).ToList(),
                _       => Blocks.ToList<Block>()
            };
        } else {
            sortedBlocks = columnName.ToLower() switch {
                "name"  => Blocks.OrderByDescending<Block, string>(x => x.Name ?? "").ToList(),
                "id"    => Blocks.OrderByDescending<Block, string>(x => x.Id ?? "").ToList(),
                "state" => Blocks.OrderByDescending<Block, bool>(x => x.IsOccupied).ToList(),
                _       => Blocks.ToList<Block>()
            };
        }

        Blocks = new ObservableCollection<Block>(sortedBlocks);

        _sortColumn = columnName;
        _isAscending = !_isAscending;
        OnPropertyChanged(nameof(Blocks));
        SetLabels();
    }

    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals("ID") ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals("SystemName") ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals("Is Occupied") ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    private async Task RefreshBlocksAsync() {
        var result = await ConnectionService.Connect(Profile.ActiveConnectionInfo);
        if (result.IsSuccess) {
            Client = result.Value;
            Client?.Disconnect();
        } else {
            Client = null;
        }
    }
}