using System.ComponentModel;
using System.Text;
using CommunityToolkit.Maui.Core.Extensions;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using Serilog;

namespace DCCPanelController.View;

public partial class PanelEditor : ContentPage {
    private readonly ILogger<PanelEditor> _logger;
    private readonly TaskCompletionSource<bool> _closeTcs = new();
    private readonly PanelEditorViewModel _viewModel;
    private bool _isPushingModal; // Flag to track modal presentation

    public PanelEditor(ILogger<PanelEditor> logger, Panel panel, ProfileService profileService) {
        _logger = logger;
        InitializeComponent();
        if (panel.Cols <= 0) panel.Cols = 18;
        if (panel.Rows <= 0) panel.Rows = 10;

        _viewModel = new PanelEditorViewModel(_logger, panel, profileService, this, PanelView) {
            GridVisible = true,
            EditMode = EditModeEnum.Move
        };
        ShowSelectedMode();

        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        _viewModel.OnBeginPushModal += OnBeginPushModal; // Subscribe to the custom event
        _viewModel.OnBeginPopModal += OnBeginPopModal;   // Subscribe to the custom event
        _viewModel.ForcePanelRefresh += ViewModelOnForcePanelRefresh;
        PanelView.TileSelected += PanelViewOnTileSelected;
        PanelView.TileChanged += PanelViewOnTileChanged;
        PanelView.TileTapped += PanelViewOnTileTapped;
        BindingContext = _viewModel;
    }

    public Task<bool> PageClosed => _closeTcs.Task;

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        _viewModel.ScreenHeight = height;
        _viewModel.ScreenWidth = width;
    }

    private void OnBeginPushModal() {
        _isPushingModal = true;
    }

    private void OnBeginPopModal() {
        _isPushingModal = false;
    }
    
    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args) {
        base.OnNavigatedFrom(args);

        if (_isPushingModal) {
            _isPushingModal = false;
        } else {
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _viewModel.OnBeginPushModal -= OnBeginPushModal; // Subscribe to the custom event
            _viewModel.OnBeginPopModal -= OnBeginPopModal;   // Subscribe to the custom event
            _viewModel.ForcePanelRefresh -= ViewModelOnForcePanelRefresh;
            PanelView.TileSelected -= PanelViewOnTileSelected;
            PanelView.TileChanged -= PanelViewOnTileChanged;
            PanelView.TileTapped -= PanelViewOnTileTapped;
            _closeTcs.TrySetResult(true); // or return data as needed
        }
    }

    private async void PanelViewOnTileTapped(object? sender, TileSelectedEventArgs e) {
        if (BindingContext is PanelEditorViewModel viewModel) {
            if (e.Tile is ITileInteractive { } tile) {
                if (e.IsSingleTap) await tile.Interact(null);
                if (e.IsDoubleTap) await tile.Secondary(null);
            }
        }
    }

    private void ViewModelOnForcePanelRefresh() {
        PanelView.ForceRefresh();
    }

    private void PanelViewOnTileSelected(object? sender, TileSelectedEventArgs e) {
        _viewModel.SelectedTiles = e.Tiles.ToObservableCollection();
        _viewModel.SetCanEditProperties();

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
            NeedsSavingText.Text = _viewModel.HavePropertiesChanged ? "Unsaved Changes" : "";
            ShowSelectedMode();
            break;

        case nameof(PanelEditorViewModel.GridVisible):
            PanelView.ShowGrid = _viewModel.GridVisible;
            PanelView.DesignMode = _viewModel.GridVisible;
            PanelView.ClearAllSelectedTiles();
            GridToolbar.IconImageSource = _viewModel.GridVisible ? "grid_on.png" : "grid_off.png";
            break;

        case nameof(PanelEditorViewModel.EditMode):
            PanelView.EditMode = _viewModel.EditMode;
            ModeToolbar.IconImageSource = _viewModel.EditMode switch {
                EditModeEnum.Move => "move.png",
                EditModeEnum.Copy => "copy.png",
                EditModeEnum.Size => "crop.png",
                _                 => "move.png"
            };
            ShowSelectedMode();
            break;
        }
    }

    private void ShowSelectedMode() {
        if (_viewModel is { } vm) {
            DisplayBorder.BackgroundColor = vm.Panel?.DisplayBackgroundColor ?? Colors.WhiteSmoke;
            EditModeText.Text = vm.EditMode switch {
                EditModeEnum.Move => "Move Tiles Mode",
                EditModeEnum.Copy => "Copy Tiles Mode",
                EditModeEnum.Size => "Resize Tiles Mode",
                _                 => ""
            };
        }
    }
}