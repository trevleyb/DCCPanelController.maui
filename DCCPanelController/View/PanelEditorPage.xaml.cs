using System.Diagnostics;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.PropertyPages;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {
    private const double MinRightPaneWidth = 75;  // Minimum width constraint for the right pane
    private const double MaxRightPaneWidth = 250; // Maximum width constraint for the right pane

    public PanelEditorPage(PanelEditorViewModel viewModel) {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();

        SetEditModeIcon(EditModeEnum.Move);
        SetGridIcon(true);
    }

    private static NavigationService NavigationService => MauiProgram.ServiceHelper.GetService<NavigationService>();
    private PanelEditorViewModel ViewModel { get; }

    private void PanelView_OnTrackPieceChanged(object? sender, ITrack track) {
        ViewModel.TrackPieceChanged();
    }

    private void PanelView_OnTrackPieceTapped(object? sender, ITrack track) {
        if (track.IsSelected) {
            PanelView.MarkTrackUnSelected(track);
        } else {
            PanelView.MarkTrackSelected(track);
        }
    }

    private void PanelView_OnTrackPieceDoubleTapped(object? sender, ITrack track) {
        PanelView.ClearSelectedTracks();
        PanelView.MarkTrackSelected(track);
        ShowEditPropertyPage(sender, EventArgs.Empty);
    }

    private void OnSymbolDragStarting(object sender, DragStartingEventArgs e) {
        if (sender is DragGestureRecognizer { BindingContext: ITrackSymbol symbol }) {
            e.Data.Properties.Add("Track", symbol);
            e.Data.Properties.Add("Source", "DisplaySymbol");
        }
    }

    private void ToggleValidation(object? sender, EventArgs e) {
        PanelView.ShowTrackErrors = !PanelView.ShowTrackErrors;
        PanelView.RebuildGrid(true);
    }

    private async void ShowPropertyPage(object? sender, EventArgs e) {
        try {
            await ShowPropertyPageAsync(sender, e);
            PanelView?.RebuildGrid(true);
        } catch (Exception ex) {
            Trace.WriteLine($"Exception {ex.Message} in Property Pages."); // TODO handle exception
        }
    }

    private async Task ShowPropertyPageAsync(object? sender, EventArgs e) {
        await NavigationService.NavigateToPopupWindow(new PanelPropertyPage(ViewModel.Panel));
    }

    private async void ShowEditPropertyPage(object? sender, EventArgs e) {
        try {
            await ShowEditPropertyPageAsync(sender, e);
            PanelView?.RebuildGrid(true);
        } catch (Exception ex) {
            Trace.WriteLine($"Exception {ex.Message} in Property Pages.");
        }
    }

    private async Task ShowEditPropertyPageAsync(object? sender, EventArgs e) {
        if (ViewModel is { HasSelectedTracks: true, CanUsePropertyPage: true }) {
            var track = ViewModel.Panel.SelectedTracks.First();
            await NavigationService.NavigateToPopupWindow(new DynamicPropertyPage(track));
            PanelView.MarkTrackUnSelected(track);

            // foreach (var track in ViewModel.Panel.SelectedTracks) {
            //     await NavigationService.NavigateToPopupWindow(new DynamicPropertyPage(track));
            //     PanelView.MarkTrackUnSelected(track);
            // }
        }
    }

    private void RotateLeft(object? sender, EventArgs e) {
        foreach (var track in ViewModel.Panel.SelectedTracks) track.RotateLeft();
    }

    private void RotateRight(object? sender, EventArgs e) {
        foreach (var track in ViewModel.Panel.SelectedTracks) track.RotateRight();
    }

    private void DeleteTrackPiece(object? sender, EventArgs e) {
        foreach (var track in ViewModel.Panel.SelectedTracks) {
            PanelView.RemoveTrackPiece(track);
        }
    }

    private void SetEditModeIcon(EditModeEnum editMode) {
        PanelView.EditMode = editMode;
        SetEditModeIcon();
    }

    private void SetEditModeIcon() {
        EditModeToolbar.IconImageSource = PanelView.EditMode switch {
            EditModeEnum.Move => "move.png",
            EditModeEnum.Copy => "copy.png",
            EditModeEnum.Size => "crop.png", // Turned off as not worked quite right
            _                 => EditModeToolbar.IconImageSource
        };
    }

    private void ChangeEditMode(object? sender, EventArgs e) {
        PanelView.EditMode = PanelView.EditMode switch {
            EditModeEnum.Move => EditModeEnum.Copy,
            EditModeEnum.Copy => EditModeEnum.Move,
            _                 => PanelView.EditMode
        };

        SetEditModeIcon();
    }

    private void ToggleGrid(object? sender, EventArgs e) {
        PanelView.ShowGrid = !PanelView.ShowGrid;
        SetGridIcon();
    }

    private void SetGridIcon(bool state) {
        PanelView.ShowGrid = state;
        SetGridIcon();
    }

    private void SetGridIcon() {
        GridButton.IconImageSource = PanelView.ShowGrid ? "grid_on.png" : "grid_off.png";
        PanelView.RebuildGrid(true);
    }

    private void ZoomIn_Clicked(object? sender, EventArgs e) {
        if (LeftPane.Scale < 3.0) LeftPane.Scale += 0.1;
    }

    private void ZoomOut_Clicked(object? sender, EventArgs e) {
        if (LeftPane.Scale > 0.5) LeftPane.Scale -= 0.1;
    }

    private void ZoomReset_Clicked(object? sender, EventArgs e) {
        LeftPane.Scale = 1.0;
    }
}