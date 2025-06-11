using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon.Client;
using DCCCommon.Events;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;

namespace DCCPanelController.View;

public partial class BlocksViewModel : Base.ConnectionViewModel {
    private const string _labelID = "ID";
    private const string _labelName = "Block";
    private const string _labelState = "Occupied?";

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;

    [ObservableProperty] private ObservableCollection<Block> _blocks;
    [ObservableProperty] private string _columnLabelID = _labelID;
    [ObservableProperty] private string _columnLabelName = _labelName;
    [ObservableProperty] private string _columnLabelState = _labelState;
    private bool _isAscending;
    private string _sortColumn = "";

    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;
    
    public BlocksViewModel(Profile profile, ConnectionService connectionService) : base(profile, connectionService) {
        Blocks = Profile.Blocks;
        IsSupported = profile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapabilitiesEnum.Blocks) ?? false;
        SetLabels();
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Block> sortedBlocks;
        if (!_isAscending) {
            sortedBlocks = columnName switch {
                _labelName  => Blocks.OrderBy<Block, string>(x => x.Name ?? "").ToList(),
                _labelID    => Blocks.OrderBy<Block, string>(x => x.Id ?? "").ToList(),
                _labelState => Blocks.OrderBy<Block, bool>(x => x.IsOccupied).ToList(),
                _           => Blocks.ToList<Block>()
            };
        } else {
            sortedBlocks = columnName switch {
                _labelName  => Blocks.OrderByDescending<Block, string>(x => x.Name ?? "").ToList(),
                _labelID    => Blocks.OrderByDescending<Block, string>(x => x.Id ?? "").ToList(),
                _labelState => Blocks.OrderByDescending<Block, bool>(x => x.IsOccupied).ToList(),
                _           => Blocks.ToList<Block>()
            };
        }

        Blocks = new ObservableCollection<Block>(sortedBlocks);

        _sortColumn = columnName;
        _isAscending = !_isAscending;
        OnPropertyChanged(nameof(Blocks));
        SetLabels();
    }

    private void SetLabels() {
        ColumnLabelID = LabelID + (_sortColumn.Equals(_labelID) ? _isAscending.GetSortDirection() : "");
        ColumnLabelName = LabelName + (_sortColumn.Equals(_labelName) ? _isAscending.GetSortDirection() : "");
        ColumnLabelState = LabelState + (_sortColumn.Equals(_labelState) ? _isAscending.GetSortDirection() : "");
    }

    [RelayCommand]
    public async Task ToggleBlockState(Block? block) {
        if (block == null) return;
        block.IsOccupied = !block.IsOccupied;
        if (!string.IsNullOrEmpty(block.Id) && IsConnected) {
            await ConnectionService.SendBlockCmdAsync(block, block.IsOccupied )!;
        }
    }
    
    [RelayCommand]
    private async Task RefreshBlocksAsync() {
        IsBusy = true;
        try {
            for (var ptr = Profile.Blocks.Count; ptr > 0; ptr--) {
                Profile.Blocks.RemoveAt(ptr - 1);
                OnPropertyChanged(nameof(Blocks));
            }
            await ConnectionService.ForceRefresh();
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }
}