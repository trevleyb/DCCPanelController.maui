using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Events;
using DCCPanelController.Model;
using DCCPanelController.Services;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.View.PropertPages;
using DCCPanelController.ViewModel;
using Color = System.Drawing.Color;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {
    private const double MinRightPaneWidth = 75;  // Minimum width constraint for the right pane
    private const double MaxRightPaneWidth = 250; // Maximum width constraint for the right pane

    private EditState _editState = EditState.None;

    private readonly PanelsViewModel _panelsViewModel;
    private Panel Panel { get; init; }
    private PanelEditorViewModel ViewModel { get; set; }

    public PanelEditorPage(PanelsViewModel panelsViewModel) {
        ArgumentNullException.ThrowIfNull(panelsViewModel, nameof(panelsViewModel));
        ArgumentNullException.ThrowIfNull(panelsViewModel.SelectedPanel, nameof(panelsViewModel.SelectedPanel));
        _panelsViewModel = panelsViewModel;
        Panel = _panelsViewModel.SelectedPanel;
        ViewModel = new PanelEditorViewModel(Panel);
        BindingContext = ViewModel;

        InitializeComponent();
        PanelView.TrackPieceTapped += OnTrackPieceTapped;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);
        PanelView.RebuildGrid(true);
    }

    protected override bool OnBackButtonPressed() {
        if (_editState == EditState.Changed) {
            Console.WriteLine("Panel was changed.");

            //    var answer = DisplayAlert("Save Changed?", "You have unsaved Changes. Do you want to save?", "Yes", "No").GetAwaiter().GetResult();
            //    if (answer) PanelsViewModel.Save();
            //    PanelsViewModel.Load();
        }
        return base.OnBackButtonPressed();
    }

    #region Support Drag and Drop of the Symbols from the Symbol Pane
    private void OnSymbolDragStarting(object sender, DragStartingEventArgs e) {
        Console.WriteLine("OnSymbolDragStarting");
        if (sender is DragGestureRecognizer { BindingContext: ITrackSymbol symbol }) {
            e.Data.Properties.Add("Track", symbol);
            e.Data.Properties.Add("Source", "Symbol");
        }
    }
    #endregion Drag and drop

    #region Handle Selecting and Actioning on a Track including multiple selections
    /// <summary>
    /// When a TrackPiece is selected, track it and keep it and tell the
    /// underlying panel to highlight it. This is to support future MULTI-SELECT
    /// mode. 
    /// </summary>
    private void OnTrackPieceTapped(object? sender, TrackSelectedEvent trackSelectedEvent) {
        switch (trackSelectedEvent.Taps) {
        case 1:
            _editState = EditState.Changed;
            trackSelectedEvent.Track?.RotateLeft();
            break;
        case 2:
            _editState = EditState.Changed;
            if (trackSelectedEvent.Track is { } trackPiece) {
                Navigation.PushModalAsync(new PropertyPage(trackPiece));
            }
            break;
        }
    }
    #endregion

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
        // If the view model has selected items, then we do the properties on 
        // those item(s). If not, then we do the main panel page properties.
        _editState = EditState.Changed;
        Navigation.PushModalAsync(new PanelPropertyPage(Panel));
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
}

public enum EditState {
    None,
    Saved,
    Changed
}