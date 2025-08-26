using System.ComponentModel;
using CommunityToolkit.Maui.Core.Extensions;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.TileSelectors;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class PanelEditor : ContentPage {
    private readonly ILogger<PanelEditor> _logger;
    private readonly PanelEditorViewModel _viewModel;
    private readonly TaskCompletionSource<bool> _closeTcs = new();
    private bool _isPushingModal; // Flag to track modal presentation

    public PanelEditor(ILogger<PanelEditor> logger, Panel panel, ProfileService profileService) {
        _logger = logger;

        InitializeComponent();
        if (panel.Cols <= 0) panel.Cols = 18;
        if (panel.Rows <= 0) panel.Rows = 10;

        _viewModel = new PanelEditorViewModel(_logger, panel, profileService, this, PanelView, BottomSheet) {
            GridVisible = true,
            EditMode = EditModeEnum.Move
        };

        PanelView.TileSelected += PanelViewOnTileSelected;
        PanelView.TileChanged += PanelViewOnTileChanged;
        PanelView.TileTapped += PanelViewOnTileTapped;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        _viewModel.ForcePanelRefresh += ViewModelOnForcePanelRefresh;

        BindingContext = _viewModel;
        AppState.Instance.IsEditingPanel = true;
        SidePaletteSelector.Panel = _viewModel.Panel;
        PillPaletteSelector.Panel = _viewModel.Panel;
    }

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        _viewModel.ScreenHeight = height;
        _viewModel.ScreenWidth = width;

        if (width < height) SetDockedSide(TileSelectorDockSide.Bottom);
        if (width >= height) SetDockedSide(TileSelectorDockSide.Side);
    }

    public Task<bool> PageClosed => _closeTcs.Task;

    private void OnBeginPushModal() {
        Console.WriteLine("Begin Push Modal");
        _isPushingModal = true;
    }

    private void OnBeginPopModal() {
        Console.WriteLine("Begin Pop Modal");
        _isPushingModal = false;
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args) {
        Console.WriteLine("Panel Editor: Navigated From");
        base.OnNavigatedFrom(args);
        AppState.Instance.IsEditingPanel = false;

        if (_isPushingModal) {
            Console.WriteLine("Pop Modal - Push Modal in progress");
            _isPushingModal = false;
        } else {
            Console.WriteLine("Pop Modal - Push Modal not in progress");
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _viewModel.ForcePanelRefresh -= ViewModelOnForcePanelRefresh;
            PanelView.TileSelected -= PanelViewOnTileSelected;
            PanelView.TileChanged -= PanelViewOnTileChanged;
            PanelView.TileTapped -= PanelViewOnTileTapped;
            _closeTcs.TrySetResult(true); // or return data as needed
        }
    }

    private void BottomSheetStateChanged(object? sender, StateChangedEventArgs e) { }
    private void BottomSheetOnStateChanged(object? sender, StateChangedEventArgs e) {
        _viewModel.BottomSheetOnStateChanged(sender, e);
    }
    
    #region Manage the showing and hiding of the Palettes
    private void PaletteDockSideChanged(object? sender, TileSelectorDockSide e) {
        SetDockedSide(e);
    }

    private void SetDockedSide(TileSelectorDockSide side) {

        switch (side) {
        case TileSelectorDockSide.Side:
            SetPaletteVisibility(84, 0);
            break;
    
        case TileSelectorDockSide.Bottom:
            SetPaletteVisibility(0, 120);
            break;

        default:
            throw new ArgumentOutOfRangeException(nameof(side), side, null);
        }
    }

    private void SetPaletteVisibility(int side, int bottom) {
        SidePalette.Width = new GridLength(side);
        BottomPalette.Height = new GridLength(bottom);
        SidePaletteContainer.IsVisible = side > 0;
        BottomPaletteContainer.IsVisible = bottom > 0;
    }
    #endregion
    
    #region Manage Tiles being selected or tapped from the Control Panel View
    private async void PanelViewOnTileTapped(object? sender, TileSelectedEventArgs e) {
        try {
            if (BindingContext is PanelEditorViewModel) {
                if (e.Tile is ITileInteractive { } tile) {
                    if (e.IsSingleTap) await tile.Interact(null);
                    if (e.IsDoubleTap) await tile.Secondary(null);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"Exception in PanelViewOnTileTapped: {ex.Message}");
        }
    }

    private async void ViewModelOnForcePanelRefresh() {
        try {
            await PanelView.ForceRefresh();
        } catch (Exception e) {
            _logger.LogCritical("Error Forcing Refresh from ViewModelOnForcePanelRefresh: {Message}", e.Message);
        }
    }

    private void PanelViewOnTileSelected(object? sender, TileSelectedEventArgs e) {
        _viewModel.SelectedTiles = e.Tiles.ToObservableCollection();
        _viewModel.SetCanEditProperties();
        var editIcon = _viewModel.SelectedTiles.Count == 0 ? "settings" : "edit";
        ToolbarIconHelper.BindIcon(EditToolbar, nameof(_viewModel.CanEditProperties), editIcon);
        SelectionText.Text = _viewModel.SelectedTiles.Count switch {
            0   => "No tiles selected",
            1   => $"Selected Tile: {_viewModel.SelectedTiles[0].Entity.EntityName}",
            > 1 => $"Multiple Selected Tiles ({_viewModel.SelectedTiles.Count})",
            _   => SelectionText.Text
        };
        if (e.IsLongTap) _viewModel.EditTilePropertiesCommand.Execute(e.Tile);
        _viewModel.CheckIfPanelChanged();
    }

    private void PanelViewOnTileChanged(object? sender, TileSelectedEventArgs e) {
        _viewModel.CheckIfPanelChanged();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(PanelEditorViewModel.HavePropertiesChanged):
            break;

        case nameof(PanelEditorViewModel.GridVisible):
            PanelView.ShowGrid = _viewModel.GridVisible;
            PanelView.DesignMode = _viewModel.GridVisible;
            PanelView.ClearAllSelectedTiles();
            var gridIcon = _viewModel.GridVisible ? "grid_on" : "grid_off";
            ToolbarIconHelper.BindIcon(GridToolbar, nameof(_viewModel.CanToggleGrid), gridIcon);
            break;

        case nameof(PanelEditorViewModel.EditMode):
            PanelView.EditMode = _viewModel.EditMode;
            var editIcon = _viewModel.EditMode switch {
                EditModeEnum.Move => "move",
                EditModeEnum.Copy => "copy",
                EditModeEnum.Size => "crop",
                _                 => "move"
            };
            ToolbarIconHelper.BindIcon(ModeToolbar, nameof(_viewModel.CanSetModes), editIcon);
            break;
        }
    }
    #endregion

}