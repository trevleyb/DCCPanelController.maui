using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class BlocksViewModel : ConnectionViewModel {
    private const string _labelID    = "ID";
    private const string _labelName  = "Block";
    private const string _labelState = "Occupied?"; 

    private readonly ProfileService _profileService;

    [ObservableProperty] private Block?                      _selectedBlock;
    [ObservableProperty] private ObservableCollection<Block> _blocks;
    [ObservableProperty] private string                      _columnLabelID    = _labelID;
    [ObservableProperty] private string                      _columnLabelName  = _labelName;
    [ObservableProperty] private string                      _columnLabelState = _labelState;
    private                      bool                        _isAscending;
    private                      ILogger<BlocksViewModel>    _logger;
    private                      string                      _sortColumn = "";

    [ObservableProperty] private bool _isBlockSelected;
    [ObservableProperty] private bool _canAddBlock;

    public BlocksViewModel(ILogger<BlocksViewModel> logger, ProfileService profileService, ConnectionService connectionService) : base(profileService, connectionService) {
        _logger = logger;
        _profileService = profileService;
        profileService.ActiveProfileChanged += (sender, args) => {
            Blocks = profileService?.ActiveProfile?.Blocks ?? throw new ArgumentNullException(nameof(profileService), "BlocksViewModel: Active profile is not defined.");
            IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Blocks) ?? false;
            SetLabels();
        };
        
        PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedBlock)) {
                IsBlockSelected = SelectedBlock != null;
            }
        };

        Blocks = profileService?.ActiveProfile?.Blocks ?? throw new ArgumentNullException(nameof(profileService), "BlocksViewModel: Active profile is not defined.");
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Blocks) ?? false;
        SetLabels();
    }

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;

    public bool IsSupported { get; private set; }
    public bool IsNotSupported => !IsSupported;

    private SfBottomSheet? _bottomSheet;
    public void SetNavigationReferences(SfBottomSheet bottomSheet) => _bottomSheet = bottomSheet;

    public void SetToolbarItems() {
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Blocks) ?? false;
        CanAddBlock = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
    }

    [RelayCommand]
    private async Task SortByColumnAsync(string columnName) {
        List<Block> sortedBlocks;
        if (!_isAscending) {
            sortedBlocks = columnName switch {
                _labelName  => Blocks.OrderBy<Block, string>(x => x.Name ?? "").ToList(),
                _labelID    => Blocks.OrderBy<Block, string>(x => x.Id ?? "").ToList(),
                _labelState => Blocks.OrderBy<Block, bool>(x => x.IsOccupied).ToList(),
                _           => Blocks.ToList<Block>(),
            };
        } else {
            sortedBlocks = columnName switch {
                _labelName  => Blocks.OrderByDescending<Block, string>(x => x.Name ?? "").ToList(),
                _labelID    => Blocks.OrderByDescending<Block, string>(x => x.Id ?? "").ToList(),
                _labelState => Blocks.OrderByDescending<Block, bool>(x => x.IsOccupied).ToList(),
                _           => Blocks.ToList<Block>(),
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
        if (!string.IsNullOrEmpty(block.Id)) {
            if (ConnectionService.Client is {State: DccClientState.Connected } client) await client.SendBlockCmdAsync(block, block.IsOccupied)!;
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
    
    [RelayCommand]
    private async Task DeleteBlockAsync(Block? block) {
        block ??= SelectedBlock;
        if (block is { }) {
            Blocks.Remove(block);
            OnPropertyChanged(nameof(Blocks));
            await _profileService.SaveAsync();
            SelectedBlock = null;
            IsBlockSelected = false;
        }
    }

    [RelayCommand]
    private async Task AddBlockAsync() {
        var block = new Block() {
            Id = TableAnalyser<Block>.GetUniqueID(Blocks.ToList<Block>()),
            Name = "New Block",
            IsEditable = true,
        };
        Blocks.Add(block);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Blocks));
        SelectedBlock = null;
        IsBlockSelected = false;
    }
    
    [RelayCommand]
    public async Task EditBlockAsync(Block? block) {
        block ??= SelectedBlock;
        try {
            if (block is { } && _bottomSheet is { } sfBottomSheet) {
                var sensors = _profileService?.ActiveProfile?.Sensors ?? [];
                var blocksEditViewModel = new BlocksEditViewModel(LogHelper.CreateLogger<BlocksEditViewModel>(), block, sensors, ConnectionService);
                sfBottomSheet.BottomSheetContent = blocksEditViewModel.CreatePropertiesView(); 
        
                if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Current.Idiom == DeviceIdiom.Phone) {
                    sfBottomSheet.ContentWidthMode = BottomSheetContentWidthMode.Full;
                } else {
                    sfBottomSheet.ContentWidthMode = BottomSheetContentWidthMode.Custom;
                    sfBottomSheet.BottomSheetContentWidth = 400;
                }

                sfBottomSheet.ShowGrabber = true;
                sfBottomSheet.EnableSwiping = true;
                sfBottomSheet.CollapsedHeight = 0;
                sfBottomSheet.CollapseOnOverlayTap = true;
                sfBottomSheet.State = BottomSheetState.HalfExpanded;
                sfBottomSheet.IsModal = true;
                sfBottomSheet.IsVisible = true;
                sfBottomSheet.Show();
            }
        } catch (Exception ex) {
            _logger.LogCritical("Error Launching Blocks Properties Page: " + ex.Message);
        }
        OnPropertyChanged(nameof(Blocks));
    }
}