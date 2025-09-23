using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core.Extensions;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View.ControlPanel;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.TileSelectors;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class PanelEditor : ContentPage {
    private readonly TaskCompletionSource<bool> _closeTcs = new();
    private readonly ILogger<PanelEditor>       _logger;
    private readonly PanelEditorViewModel       _viewModel;

    private TileSelectorDockSide? _currentState;

    public PanelEditor(ILogger<PanelEditor> logger, Panel panel, ProfileService profileService) {
        _logger = logger;

        InitializeComponent();
        if (panel.Cols <= 0) panel.Cols = 18;
        if (panel.Rows <= 0) panel.Rows = 10;

        _viewModel = new PanelEditorViewModel(_logger, panel, profileService, this, PanelView, PanelEditorContainer) {
            GridVisible = true,
            EditMode = EditModeEnum.Move,
        };
        
        PanelView.TileSelected += PanelViewOnTileSelected;
        PanelView.TileChanged += PanelViewOnTileChanged;
        PanelView.TileTapped += PanelViewOnTileTapped;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        BindingContext = _viewModel;
        AppStateService.Instance.IsEditingPanel = true;
        SidePaletteSelector.Panel = _viewModel.Panel;
        PillPaletteSelector.Panel = _viewModel.Panel;

        AppStateService.Instance.SelectedTileSet += InstanceOnSelectedTileSet;
        AppStateService.Instance.SelectedTileCleared += InstanceOnSelectedTileCleared;
    }

    public Task<bool> PageClosed => _closeTcs.Task;

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        if (width <= 0 || height <= 0) return;
        UpdateScreenDimensions(width, height);
    }

    private void UpdateScreenDimensions(double width, double height) {
        _viewModel.ScreenHeight = height;
        _viewModel.ScreenWidth = width;

        var newState = width <= height ? TileSelectorDockSide.Bottom : TileSelectorDockSide.Side;
        if (newState != _currentState) {
            _currentState = SetDockedSide(newState);
        }
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args) {
        base.OnNavigatedFrom(args);

        if (!_viewModel.ExitViaBackButton) {
            Debug.WriteLine("WARNING! Exiting Editor NOT via Back Button. No SAVE");
        }

        PanelView.TileSelected -= PanelViewOnTileSelected;
        PanelView.TileChanged -= PanelViewOnTileChanged;
        PanelView.TileTapped -= PanelViewOnTileTapped;
        _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;

        _closeTcs.TrySetResult(true); // or return data as needed
        AppStateService.Instance.IsEditingPanel = false;
    }

    #region Manage the showing and hiding of the Palettes
    private void PaletteDockSideChanged(object? sender, TileSelectorDockSide e) => SetDockedSide(e);

    private TileSelectorDockSide SetDockedSide(TileSelectorDockSide side) {
        switch (side) {
            case TileSelectorDockSide.Side:
                BottomPaletteContainer.HeightRequest = 0;
                SidePaletteContainer.WidthRequest = 100;
            break;

            case TileSelectorDockSide.Bottom:
                BottomPaletteContainer.HeightRequest = 120;
                SidePaletteContainer.WidthRequest = 0;
            break;

            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, null);
        }
        return side;
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
            Debug.WriteLine($"Exception in PanelViewOnTileTapped: {ex.Message}");
        }
    }

    private async void ViewModelOnForcePanelRefresh() {
        try {
            await PanelView.ForceRefreshAsync();
        } catch (Exception e) {
            _logger.LogCritical("Error Forcing Refresh from ViewModelOnForcePanelRefresh: {Message}", e.Message);
        }
    }

    private void InstanceOnSelectedTileCleared() {
        PanelView.ClearAllSelectedTiles();
        PanelViewOnTileSelected([]);
    }

    private void InstanceOnSelectedTileSet(ITile obj) {
        PanelView.ClearAllSelectedTiles();
        PanelViewOnTileSelected([]);
    }

    private void PanelViewOnTileSelected(object? sender, TileSelectedEventArgs e) => PanelViewOnTileSelected(e.Tiles);

    private void PanelViewOnTileSelected(HashSet<ITile> tiles) {
        _viewModel.SelectedTiles = tiles.ToObservableCollection();
        _viewModel.CheckIfCanLinkTiles();
        if (AppStateService.Instance.SelectedTile is { } selectedTile) {
            SelectionText.Text = $"Place Tile: {selectedTile.Entity.EntityName}";
        } else {
            switch (_viewModel.SelectedTiles.Count) {
                case 0:
                    SelectionText.Text = "No tiles selected";
                break;

                case 1:
                    var selectedEntity = _viewModel.SelectedTiles[0].Entity;
                    SelectionText.Text = $"Selected Tile: {selectedEntity.EntityName} @ Layer:{selectedEntity.Layer} ";
                    if (selectedEntity is TurnoutEntity turnout) {
                        SelectionText.Text += $" ({turnout.Id ?? "Undefined"} [DCC={turnout.Turnout?.Name}@{turnout.Turnout?.Id ?? "000"}])";
                    } else if (selectedEntity is IEntityID entityID) {
                        SelectionText.Text += $" ({entityID.Id})";
                    }
                break;

                case> 1:
                    SelectionText.Text = $"Multiple Selected Tiles ({_viewModel.SelectedTiles.Count})";
                break;

                default:
                    SelectionText.Text = SelectionText.Text;
                break;
            }
        }

        ChangesText.Text = _viewModel.HavePropertiesChanged ? "Changes" : "No Changes";
        ChangesText.TextColor = _viewModel.HavePropertiesChanged ? Colors.Red : StyleHelper.FromStyle("Primary");
        _viewModel.CheckIfPanelChanged();
    }

    private void PanelViewOnTileChanged(object? sender, TileSelectedEventArgs e) => _viewModel.CheckIfPanelChanged();

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
            case nameof(PanelEditorViewModel.HavePropertiesChanged):
            break;

            case nameof(PanelEditorViewModel.GridVisible):
                PanelView.ShowGrid = _viewModel.GridVisible;
                var gridIcon = _viewModel.GridVisible ? "grid_on" : "grid_off";
                ToolbarIconHelper.BindIcon(GridToolbar, nameof(_viewModel.CanToggleGrid), gridIcon);
            break;

            case nameof(PanelEditorViewModel.EditMode):
                PanelView.EditMode = _viewModel.EditMode;
                var editIcon = _viewModel.EditMode switch {
                    EditModeEnum.Move => "move",
                    EditModeEnum.Copy => "copy",
                    EditModeEnum.Size => "crop",
                    _                 => "move",
                };
                ToolbarIconHelper.BindIcon(ModeToolbar, nameof(_viewModel.CanSetModes), editIcon);
            break;
        }
    }
    #endregion
}