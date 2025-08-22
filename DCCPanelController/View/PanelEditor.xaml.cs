using System.ComponentModel;
using CommunityToolkit.Maui.Core.Extensions;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.TileSelectors;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace DCCPanelController.View;

public partial class PanelEditor : ContentPage {
    private readonly ILogger<PanelEditor> _logger;
    private readonly TaskCompletionSource<bool> _closeTcs = new();
    private readonly PanelEditorViewModel _viewModel;
    private bool _isPushingModal; // Flag to track modal presentation

    public PanelEditor(ILogger<PanelEditor> logger, Panel panel, ProfileService profileService) {
        _logger = logger;
        _viewModel = new PanelEditorViewModel(_logger, panel, profileService, this, PanelView, BottomSheet) {
            GridVisible = true,
            EditMode = EditModeEnum.Move
        };
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        _viewModel.ForcePanelRefresh += ViewModelOnForcePanelRefresh;

        InitializeComponent();
        if (panel.Cols <= 0) panel.Cols = 18;
        if (panel.Rows <= 0) panel.Rows = 10;

        PanelView.TileSelected += PanelViewOnTileSelected;
        PanelView.TileChanged += PanelViewOnTileChanged;
        PanelView.TileTapped += PanelViewOnTileTapped;

        BindingContext = _viewModel;
        AppState.Instance.IsEditingPanel = true;
        SetDockedSide(TileSelectorDockSide.Middle);
    }

    public Task<bool> PageClosed => _closeTcs.Task;

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        _viewModel.ScreenHeight = height;
        _viewModel.ScreenWidth = width;
    }

    private void OnBeginPushModal() {
        Console.WriteLine("Begin Push Modal");
        _isPushingModal = true;
    }

    private void OnBeginPopModal() {
        Console.WriteLine("Begin Pop Modal");
        _isPushingModal = false;
    }

    // private SideSelectorPanel SideSelector(TileSelectorDockSide side) {
    //     if (_sideSelectorPanel is null) {
    //         _sideSelectorPanel = new SideSelectorPanel() {
    //             HorizontalOptions = LayoutOptions.Fill,
    //             DockSide = side,
    //         };
    //         _sideSelectorPanel.OnDockSideChanged += PaletteDockSideChanged;
    //         _sideSelectorPanel.Panel = _viewModel.Panel;
    //     }
    //     _sideSelectorPanel.DockSide = side;
    //     return _sideSelectorPanel;
    // }

    private void SetDockedSide(TileSelectorDockSide side) {

        // Unregister events before we clear the content
        if (LeftPaletteContent.Content is SideSelectorPanel l) l.OnDockSideChanged -= PaletteDockSideChanged;
        if (RightPaletteContent.Content is SideSelectorPanel r) r.OnDockSideChanged -= PaletteDockSideChanged;
        if (MiddlePaletteContent.Content is PillSelectorPanel m) m.OnDockSideChanged -= PaletteDockSideChanged;
        
        LeftPaletteContent.Content = null;
        RightPaletteContent.Content = null;
        MiddlePaletteContent.Content = null;
        
        switch (side) {
        case TileSelectorDockSide.Left:
            SetPaletteVisibility(80,0,0);
            var leftPalette = new SideSelectorPanel() {
                DockSide = TileSelectorDockSide.Left,
                HorizontalOptions = LayoutOptions.Center,
                Panel = _viewModel.Panel,
            };
            leftPalette.OnDockSideChanged += PaletteDockSideChanged;
            LeftPaletteContent.Content = leftPalette;
            break;

        case TileSelectorDockSide.Right:
            SetPaletteVisibility(0,0,80);
            var rightPalette = new SideSelectorPanel() {
                DockSide = TileSelectorDockSide.Right,
                HorizontalOptions = LayoutOptions.Center,
                Panel = _viewModel.Panel,
            };
            rightPalette.OnDockSideChanged += PaletteDockSideChanged;
            RightPaletteContent.Content = rightPalette;
            break;

        case TileSelectorDockSide.Middle:
            SetPaletteVisibility(0,120,0);
            var middlePalette = new PillSelectorPanel() {
                HorizontalOptions = LayoutOptions.Center,
                Panel = _viewModel.Panel,
            };
            middlePalette.OnDockSideChanged += PaletteDockSideChanged;
            MiddlePaletteContent.Content = middlePalette;
            break;
        }
    }

    private void SetPaletteVisibility(int left, int middle, int right) {
        LeftSidePalette.Width = new GridLength(left);
        MiddlePalette.Height = new GridLength(middle);
        RightSidePalette.Width = new GridLength(right);

        LeftSideContainer.IsVisible = left > 0;
        MiddleSideContainer.IsVisible = middle > 0;
        RightSideContainer.IsVisible = right > 0;
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args) {
        base.OnNavigatedFrom(args);

        if (_isPushingModal) {
            Console.WriteLine("Pop Modal");
            _isPushingModal = false;
        } else {
            Console.WriteLine("Close Modal");
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _viewModel.ForcePanelRefresh -= ViewModelOnForcePanelRefresh;
            PanelView.TileSelected -= PanelViewOnTileSelected;
            PanelView.TileChanged -= PanelViewOnTileChanged;
            PanelView.TileTapped -= PanelViewOnTileTapped;
            _closeTcs.TrySetResult(true); // or return data as needed
            AppState.Instance.IsEditingPanel = false;
        }
    }

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

    private void BottomSheetStateChanged(object? sender, StateChangedEventArgs e) {
        Console.WriteLine($"Bottom Sheet State = {e.NewState}");
    }

    private void PaletteDockSideChanged(object? sender, TileSelectorDockSide e) {
        SetDockedSide(e);
    }
}