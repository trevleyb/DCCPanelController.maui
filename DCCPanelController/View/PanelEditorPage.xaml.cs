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

    private Panel Panel => ViewModel.Panel;
    public PanelEditorViewModel ViewModel { get; }

    public PanelEditorPage(Panel panel) {
        ArgumentNullException.ThrowIfNull(panel, nameof(Panel));
        ViewModel = new PanelEditorViewModel(panel);
        BindingContext = ViewModel;
        InitializeComponent();

        ViewModel.HasSelectedTracks = Panel.HasSelectedTracks;
        ViewModel.CanUsePropertyPage = Panel.SelectedTracks.Count <= 1;
        ViewModel.CloseRequested += async (sender, e) => {
            await Navigation.PopAsync();
        };
    }

    private void PanelView_OnTrackPieceChanged(object? sender, ITrackPiece track) {
        ViewModel.HasSelectedTracks = Panel?.HasSelectedTracks ?? false;
        ViewModel.CanUsePropertyPage = Panel?.SelectedTracks?.Count <= 1;
        ViewModel.EditState = EditState.Changed;
    }

    private void PanelView_OnTrackPieceTapped(object? sender, ITrackPiece track) {
        Console.WriteLine($"In Edit Mode: Track {track.Name} was tapped");
        if (track.IsSelected) {
            PanelView.MarkTrackUnSelected(track);
        } else {
            PanelView.MarkTrackSelected(track);
        }
    }
    
    private void OnSymbolDragStarting(object sender, DragStartingEventArgs e) {
        Console.WriteLine("OnSymbolDragStarting");
        if (sender is DragGestureRecognizer { BindingContext: ITrackSymbol symbol }) {
            e.Data.Properties.Add("Track", symbol);
            e.Data.Properties.Add("Source", "DisplaySymbol");
        }
    }

    private void SavePanelAndExit(object? sender, EventArgs e) {
        ViewModel.EditState = EditState.Saved;
        ViewModel.Save();
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
        ViewModel.EditState = EditState.Changed;
        if (Panel.HasSelectedTracks) {
            foreach (var track in Panel.SelectedTracks) {
                await NavigationService.NavigateToPopupWindow(new DynamicPropertyPage(track));
                //await Navigation.PushModalAsync(new DynamicPropertyPage(track), true);
                PanelView.MarkTrackUnSelected(track);
            }
        } else {
            await NavigationService.NavigateToPopupWindow(new PanelPropertyPage(Panel));
            //await Navigation.PushModalAsync(new PanelPropertyPage(Panel),true);
        }
    }
    
    /*
    private void DropTrackInTrash(object? sender, DropEventArgs e) {
        Console.WriteLine("Drop Track In Trash");
        e.Data.Properties.TryGetValue("Source", out var source);
        e.Data.Properties.TryGetValue("Track", out var track);
        Console.WriteLine($"Deleting track {track}");
        if (source is string && source.Equals("Panel") && track is ITrackPiece trackPiece) Panel.Tracks.Remove(trackPiece);
        DragTrashIcon.BackgroundColor = Colors.Transparent;
        PanelView.RebuildGrid(true);
        ViewModel.EditState = EditState.Changed;
    }

    private void DropTrackInTrashHoverOver(object? sender, DragEventArgs e) {
        Console.WriteLine("Drop Track In Trash Hover Over");
        e.Data.Properties.TryGetValue("Source", out var source);
        if (source is string && source.Equals("Panel")) DragTrashIcon.BackgroundColor = Colors.Red;
    }

    private void DropTrackInTrashHoverLeave(object? sender, DragEventArgs e) {
        Console.WriteLine("Drop Track In Trash Hover Leave");
        DragTrashIcon.BackgroundColor = Colors.White;
    }
    */
    
    private void RotateLeft(object? sender, EventArgs e) {
        foreach (var track in Panel.SelectedTracks) track.RotateLeft();
        ViewModel.EditState = EditState.Changed;
    }

    private void RotateRight(object? sender, EventArgs e) {
        foreach (var track in Panel.SelectedTracks) track.RotateRight();
        ViewModel.EditState = EditState.Changed;
    }

    private void DeleteTrackPiece(object? sender, EventArgs e) {
        foreach (var track in Panel.SelectedTracks) {
            PanelView.RemoveTrackPiece(track);
        }
        ViewModel.EditState = EditState.Changed;
    }
}

public enum EditModeEum { Move, Copy }
