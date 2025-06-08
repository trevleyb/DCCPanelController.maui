using System.ComponentModel;
using CommunityToolkit.Maui.Core.Extensions;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditor : ContentPage {
    private readonly TaskCompletionSource<bool> _closeTcs = new();
    private readonly PanelEditorViewModel _viewModel;
    private bool _isPushingModal; // Flag to track modal presentation

    public PanelEditor(Panel panel) {
        InitializeComponent();
        if (panel.Cols <= 0) panel.Cols = 18;
        if (panel.Rows <= 0) panel.Rows = 10;

        _viewModel = new PanelEditorViewModel(panel, Navigation);
        _viewModel.GridVisible = true;
        _viewModel.EditMode = EditModeEnum.Move;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        _viewModel.OnBeginPushModal += OnBeginPushModal; // Subscribe to the custom event
        _viewModel.OnBeginPopModal += OnBeginPopModal; // Subscribe to the custom event
        PanelView.TileSelected += PanelViewOnTileSelected;

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

    protected override async void OnDisappearing() {
        base.OnDisappearing();

        if (_isPushingModal) {
            _isPushingModal = false;
        } else {
            if (_viewModel.Panel is { } panel) {
                try {
                    panel.Base64Image = await PanelView.GetThumbnailAsync();
                    await _viewModel.SaveAsync();
                    _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
                    _viewModel.OnBeginPushModal -= OnBeginPushModal; // Subscribe to the custom event
                    _viewModel.OnBeginPopModal -= OnBeginPopModal;   // Subscribe to the custom event
                    PanelView.TileSelected -= PanelViewOnTileSelected;
                } catch (Exception ex) {
                    Console.WriteLine($"Error exiting Panel and capturing Panel Image: {ex.Message}");
                }
            }
            _closeTcs.TrySetResult(true); // or return data as needed
        }
    }

    private void PanelViewOnTileSelected(object? sender, TileSelectedEventArgs e) {
        _viewModel.SelectedTiles = e.Tiles.ToObservableCollection();
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(PanelEditorViewModel.GridVisible):
            PanelView.ShowGrid = _viewModel.GridVisible;
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
            break;
        }
    }
}