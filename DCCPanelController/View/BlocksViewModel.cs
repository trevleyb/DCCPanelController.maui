using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using Microsoft.Extensions.Logging;

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
    
    private readonly ProfileService _profileService;
    private ILogger<BlocksViewModel> _logger;
    
    public BlocksViewModel(ILogger<BlocksViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        Blocks = profileService?.ActiveProfile?.Blocks ?? throw new ArgumentNullException(nameof(profileService),"BlocksViewModel: Active profile is not defined.");
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Blocks) ?? false;
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
            if (ConnectionService.Client is { } client) await client.SendBlockCmdAsync(block, block.IsOccupied )!;
        }
    }
    
    [RelayCommand]
    private async Task RefreshBlocksAsync() {
        IsBusy = true;
        try {
            if (_profileService?.ActiveProfile is { } profile) profile.RefreshBlocks();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
        } catch { /* ignored */
        } finally {
            IsBusy = false;
        }
    }
}