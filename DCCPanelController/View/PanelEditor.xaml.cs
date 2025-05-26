using System.ComponentModel;
using CommunityToolkit.Maui.Core.Extensions;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditor : ContentPage {
    private readonly PanelEditorViewModel _viewModel;
    private readonly TaskCompletionSource<bool> _closeTcs = new TaskCompletionSource<bool>();
    public Task<bool> PageClosed => _closeTcs.Task;

    public PanelEditor(Panel panel) {
        InitializeComponent();
        if (panel.Cols <=0 ) panel.Cols = 18;
        if (panel.Rows <=0 ) panel.Rows = 10;
        
        _viewModel = new PanelEditorViewModel(panel, Navigation);
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        _viewModel.GridVisible = true;
        _viewModel.EditMode = EditModeEnum.Move;
        PanelView.TileSelected += PanelViewOnTileSelected;
        BindingContext = _viewModel;
    }

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        if (_viewModel is { } vm) {
            vm.ScreenHeight = height;
            vm.ScreenWidth = width;
        }
    }

    protected override async void OnDisappearing() {
        base.OnDisappearing();
        var panel = _viewModel.Panel;
        if (panel is not null) {
            panel.Base64Image = await PanelView.GetThumbnailAsync();
            _viewModel?.Panel?.Panels?.Profile?.SaveAsync();
        }
        _closeTcs.TrySetResult(true); // or return data as needed
    }

    private void PanelViewOnTileSelected(object? sender, TileSelectedEventArgs e) {
        _viewModel.SelectedTiles = e.Tiles.ToObservableCollection(); 
        OnPropertyChanged(nameof(_viewModel.HasSelectedEntities));;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"Panel Editor Property Changed {e.PropertyName}");
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
                _                 => "move.png",
            };
            break;
        }
    }
}