using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCClients;
using DCCCommon.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using RouteStateEnum = DCCPanelController.Models.DataModel.Entities.RouteStateEnum;

namespace DCCPanelController.View;

public partial class BlocksViewModel : ConnectionViewModel {
    private bool _isAscending;
    private string _sortColumn = "";

    private const string LabelID = "ID";
    private const string LabelName = "Block";
    private const string LabelState = "Is Occupied?";

    [ObservableProperty] private string _columnLabelID = LabelID;
    [ObservableProperty] private string _columnLabelName = LabelName;
    [ObservableProperty] private string _columnLabelState = LabelState;
    [ObservableProperty] private ObservableCollection<Block> _blocks;

    public BlocksViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        Blocks = Profile.Blocks;
        SetLabels();
    }

    private void ClientOnRouteMsgReceived(object? sender, DccOccupancyArgs e) {
        if (Blocks.Any(x => x.Id == e.BlockId)) {
            var block = Blocks.First(x => x.Id == e.BlockId);
            block.IsOccupied = e.IsOccupied;
        } else {
            Blocks.Add(new Block { Id = e.BlockId, IsOccupied = e.IsOccupied });
        }
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
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
        try {
            IsBusy = true;
            ConnectionService.ForceRefresh();
        } catch (Exception ex) {
            Console.WriteLine($"Unable to force refresh the blocks: {ex.Message}");
        } finally {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ClearAllAsync() {
        IsBusy = true;
        try {
            if (await AskUserToConfirm("Reset all Blocks?", "This wll remove all Blocks previously loaded from a Server and reload them from the Connected Server. Are you sure you want to do this?")) {
                for (var ptr = Profile.Blocks.Count; ptr > 0; ptr--) {
                    Profile.Blocks.RemoveAt(ptr - 1);
                    OnPropertyChanged(nameof(Blocks));
                }
                await RefreshBlocksAsync();
            }
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }
}