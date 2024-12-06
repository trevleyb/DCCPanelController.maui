using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.View.PropertPages;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {
    private const double MinRightPaneWidth = 75;  // Minimum width constraint for the right pane
    private const double MaxRightPaneWidth = 250; // Maximum width constraint for the right pane
    private readonly PanelsViewModel _panelsViewModel;
    private EditModeEum _editMode = EditModeEum.Move;
    private EditState _editState = EditState.None;

    public PanelEditorPage(PanelsViewModel panelsViewModel) {
        ArgumentNullException.ThrowIfNull(panelsViewModel, nameof(panelsViewModel));
        ArgumentNullException.ThrowIfNull(panelsViewModel.SelectedPanel, nameof(panelsViewModel.SelectedPanel));
        _panelsViewModel = panelsViewModel;
        Panel = _panelsViewModel.SelectedPanel;
        ViewModel = new PanelEditorViewModel(Panel);
        BindingContext = ViewModel;
        InitializeComponent();
    }

    private Panel Panel { get; }
    private PanelEditorViewModel ViewModel { get; }

    //protected override void OnNavigatedTo(NavigatedToEventArgs args) {
    //    base.OnNavigatedTo(args);
    //    PanelView.RebuildGrid(true);
    //}

    private void ChangeEditMode(object? sender, EventArgs e) {
        _editMode = _editMode switch {
            EditModeEum.Move => EditModeEum.Copy,
            EditModeEum.Copy => EditModeEum.Move,
            _                => EditModeEum.Move,
        };
        PanelView.EditMode = _editMode;

        EditModeToolItem.IconImageSource = _editMode switch {
            EditModeEum.Move => "shape_intersect.png",
            EditModeEum.Copy => "shape_except.png",
            _                => "shape_intersect.png",
        };
    }

    protected override bool OnBackButtonPressed() {
        if (_editState == EditState.Changed) {
            Console.WriteLine("Panel was changed.");
                var answer = DisplayAlert("Save Changed?", "You have unsaved Changes. Do you want to save?", "Yes", "No").GetAwaiter().GetResult();
                //if (answer) PanelsViewModel.Save();
                //PanelsViewModel.Load();
        }
        return base.OnBackButtonPressed();
    }

    private void OnSymbolDragStarting(object sender, DragStartingEventArgs e) {
        Console.WriteLine("OnSymbolDragStarting");
        if (sender is DragGestureRecognizer { BindingContext: ITrackSymbol symbol }) {
            e.Data.Properties.Add("Track", symbol);
            e.Data.Properties.Add("Source", "Symbol");
        }
    }

    private void SavePanelAndExit(object? sender, EventArgs e) {
        _panelsViewModel.Save();
        _editState = EditState.Saved;
    }

    private void ToggleGrid(object? sender, EventArgs e) {
        PanelView.ShowGrid = !PanelView.ShowGrid;
        PanelView.RebuildGrid(true);
    }

    private void ToggleValidation(object? sender, EventArgs e) {
        PanelView.ShowTrackErrors = !PanelView.ShowTrackErrors;
        PanelView.RebuildGrid(true);
    }

    private void ShowPropertyPage(object? sender, EventArgs e) {
        _ = ShowPropertyPageAsync(sender, e);
    }

    private async Task ShowPropertyPageAsync(object? sender, EventArgs e) {
        // If the view model has selected items, then we do the properties on 
        // those item(s). If not, then we do the main panel page properties.
        _editState = EditState.Changed;
        if (Panel.HasSelectedTracks) {
            foreach (var track in Panel.SelectedTracks) {
                await Navigation.PushModalAsync(new DynamicPropertyPage(track));
                PanelView.MarkTrackUnSelected(track);
            }
        } else {
            await Navigation.PushModalAsync(new PanelPropertyPage(Panel));
        }
        PanelView?.RebuildGrid(true);
    }

    
    
    private void DropTrackInTrash(object? sender, DropEventArgs e) {
        Console.WriteLine("Drop Track In Trash");
        e.Data.Properties.TryGetValue("Source", out var source);
        e.Data.Properties.TryGetValue("Track", out var track);
        Console.WriteLine($"Deleting track {track}");
        if (source is string && source.Equals("Panel") && track is ITrackPiece trackPiece) Panel.Tracks.Remove(trackPiece);
        DragTrashIcon.BackgroundColor = Colors.Transparent;
        PanelView.RebuildGrid(true);
        _editState = EditState.Changed;
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

    private void RotateLeft(object? sender, EventArgs e) {
        foreach (var track in Panel.SelectedTracks) track.RotateLeft();
    }

    private void RotateRight(object? sender, EventArgs e) {
        foreach (var track in Panel.SelectedTracks) track.RotateRight();
    }
}

public enum EditState {
    None,
    Saved,
    Changed
}

public enum EditModeEum { Move, Copy }
