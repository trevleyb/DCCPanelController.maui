using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Clients;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Base;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class BlocksViewModel : TablesViewModel<Block>
{
    private const string _labelID     = "ID";
    private const string _labelName   = "Block";
    private const string _labelState  = "Occupied?";
    private const string _labelSensor = "Sensor";

    private readonly ILogger<BlocksViewModel> _logger;

    [ObservableProperty] private Block? _selectedBlock;
    [ObservableProperty] private bool _isBlockSelected;
    [ObservableProperty] private bool _canAddBlock;

    [ObservableProperty] private string _columnLabelID     = _labelID;
    [ObservableProperty] private string _columnLabelName   = _labelName;
    [ObservableProperty] private string _columnLabelState  = _labelState;
    [ObservableProperty] private string _columnLabelSensor = _labelSensor;

    private SfBottomSheet? _bottomSheet;

    public BlocksViewModel(ILogger<BlocksViewModel> logger, ProfileService profileService, ConnectionService connectionService)
        : base(profileService, connectionService)
    {
        _logger = logger;

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SelectedBlock))
                IsBlockSelected = SelectedBlock != null;
        };
    }

    public ObservableCollection<Block> Blocks
    {
        get => Items;
        private set => Items = value;
    }

    public string LabelID => _labelID;
    public string LabelName => _labelName;
    public string LabelState => _labelState;
    public string LabelSensor => _labelSensor;

    public void SetNavigationReferences(SfBottomSheet bottomSheet) => _bottomSheet = bottomSheet;

    public void SetToolbarItems()
    {
        IsSupported = _profileService.ActiveProfile?.Settings?.ClientSettings?.Capabilities.Contains(DccClientCapability.Blocks) ?? false;
        CanAddBlock = _profileService.ActiveProfile?.Settings?.ClientSettings?.SupportsManualEntries == true && IsSupported;
    }

    protected override string DefaultSortKey => _labelName;

    protected override ObservableCollection<Block> ResolveCollection(Profile profile) => profile.Blocks;

    protected override IReadOnlyDictionary<string, Func<Block, IComparable>> Sorters => new Dictionary<string, Func<Block, IComparable>>
    {
        [_labelName]   = x => x.Name ?? "",
        [_labelID]     = x => x.Id ?? "",
        [_labelSensor] = x => x.Sensor ?? "",
        [_labelState]  = x => x.IsOccupied
    };

    protected override void UpdateColumnLabels()
    {
        ColumnLabelID = LabelWithArrow(_labelID, _labelID);
        ColumnLabelName = LabelWithArrow(_labelName, _labelName);
        ColumnLabelState = LabelWithArrow(_labelState, _labelState);
        ColumnLabelSensor = LabelWithArrow(_labelSensor, _labelSensor);
    }

    protected override void OnItemsRebound() => OnPropertyChanged(nameof(Blocks));

    [RelayCommand]
    public async Task ToggleBlockState(Block? block)
    {
        if (block == null) return;
        block.IsOccupied = !block.IsOccupied;
        if (!string.IsNullOrEmpty(block.Id))
        {
            if (ConnectionService.Client is { State: DccClientState.Connected } client)
                await client.SendBlockCmdAsync(block, block.IsOccupied)!;
        }
    }

    [RelayCommand]
    private async Task RefreshBlocksAsync()
    {
        IsBusy = true;
        try
        {
            _profileService.ActiveProfile?.RefreshBlocks();
            if (ConnectionService.Client is { } client) await client.ForceRefreshAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteBlockAsync(Block? block)
    {
        block ??= SelectedBlock;
        if (block is null) return;
        Blocks.Remove(block);
        await _profileService.SaveAsync();
        OnPropertyChanged(nameof(Blocks));
        SelectedBlock = null;
        IsBlockSelected = false;
    }

    [RelayCommand]
    private async Task AddBlockAsync()
    {
        var block = new Block
        {
            Id = TableAnalyser<Block>.GetUniqueID(Blocks.ToList()),
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
    public async Task EditBlockAsync(Block? block)
    {
        block ??= SelectedBlock;
        try
        {
            if (block is { } && _bottomSheet is { } sheet)
            {
                var sensors = _profileService.ActiveProfile?.Sensors ?? [];
                var vm = new BlocksEditViewModel(LogHelper.CreateLogger<BlocksEditViewModel>(), block, sensors, ConnectionService);
                sheet.BottomSheetContent = vm.CreatePropertiesView();

                sheet.ContentWidthMode = (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
                    ? BottomSheetContentWidthMode.Full
                    : BottomSheetContentWidthMode.Custom;
                if (sheet.ContentWidthMode == BottomSheetContentWidthMode.Custom)
                    sheet.BottomSheetContentWidth = 400;

                sheet.ShowGrabber = true;
                sheet.EnableSwiping = true;
                sheet.CollapsedHeight = 0;
                sheet.CollapseOnOverlayTap = true;
                sheet.State = BottomSheetState.HalfExpanded;
                sheet.IsModal = true;
                sheet.IsVisible = true;
                sheet.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Error Launching Blocks Properties Page: " + ex.Message);
        }
        OnPropertyChanged(nameof(Blocks));
    }
}
