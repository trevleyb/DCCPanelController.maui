using System.ComponentModel;
using System.Diagnostics;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services.NavigationService;
using DCCPanelController.View.PropertPages;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {
    private const double MinRightPaneWidth = 75;  // Minimum width constraint for the right pane
    private const double MaxRightPaneWidth = 250; // Maximum width constraint for the right pane
    private static NavigationService NavigationService => MauiProgram.ServiceHelper.GetService<NavigationService>();
    private PanelEditorViewModel ViewModel { get; }

    public PanelEditorPage(PanelEditorViewModel viewModel) {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();
    }

    private void PanelView_OnTrackPieceChanged(object? sender, ITrackPiece track) {
        ViewModel.TrackPieceChanged();
    }

    private void PanelView_OnTrackPieceTapped(object? sender, ITrackPiece track) {
        if (track.IsSelected) {
            PanelView.MarkTrackUnSelected(track);
        } else {
            PanelView.MarkTrackSelected(track);
        }
    }
    
    private void OnSymbolDragStarting(object sender, DragStartingEventArgs e) {
        if (sender is DragGestureRecognizer { BindingContext: ITrackSymbol symbol }) {
            e.Data.Properties.Add("Track", symbol);
            e.Data.Properties.Add("Source", "DisplaySymbol");
        }
    }
    
    private void ToggleGrid(object? sender, EventArgs e) {
        PanelView.ShowGrid = !PanelView.ShowGrid;
        GridButton.IconImageSource = PanelView.ShowGrid ? "grid_on.png" : "grid_off.png";
        PanelView.RebuildGrid(true);
    }

    //private void ToggleValidation(object? sender, EventArgs e) {
    //    PanelView.ShowTrackErrors = !PanelView.ShowTrackErrors;
    //    PanelView.RebuildGrid(true);
    //}

    private async void ShowPropertyPage(object? sender, EventArgs e) {
        try {
            await ShowPropertyPageAsync(sender, e);
            PanelView?.RebuildGrid(true);
        } catch (Exception ex) {
            Trace.WriteLine($"Exception {ex.Message} in Property Pages."); // TODO handle exception
        }
    }

    private async Task ShowPropertyPageAsync(object? sender, EventArgs e) {
        // If the view model has selected items, then we do the properties on 
        // those item(s). If not, then we do the main panel page properties.
        if (ViewModel.HasSelectedTracks) {
            foreach (var track in ViewModel.Panel.SelectedTracks) {
                await NavigationService.NavigateToPopupWindow(new DynamicPropertyPage(track));
                PanelView.MarkTrackUnSelected(track);
            }
        } else {
            await NavigationService.NavigateToPopupWindow(new PanelPropertyPage(ViewModel.Panel));
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
}
