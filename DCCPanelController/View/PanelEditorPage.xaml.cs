using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Events;
using DCCPanelController.Model;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.ViewModel;
using Color = System.Drawing.Color;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {

    private const double MinRightPaneWidth = 100; // Minimum width constraint for the right pane
    private const double MaxRightPaneWidth = 300; // Maximum width constraint for the right pane

    private ITrackPiece? _selectedTrack;
    private bool _isDragging = false;
    private double _initialX = 0;
    
    private Panel Panel { get; set; }
    private PanelEditorViewModel ViewModel { get; set; }

    public PanelEditorPage(Panel panel) {
        Panel = panel;
        ViewModel = new PanelEditorViewModel(panel);
        BindingContext = ViewModel;
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        PanelView.TrackPieceTapped += OnTrackPieceTapped;
    
        LeftColumn.Width = new GridLength(1, GridUnitType.Star);        // Left pane takes up remaining space
        RightColumn.Width = new GridLength(200, GridUnitType.Absolute); // Initial width of right pane set to 200
        AdjustColumnCount();
    }
    
    #region Support Drag and Drop of the Symbols from the Symbol Pane
    private void OnSymbolDragStarting(object sender, DragStartingEventArgs e) {
        if (sender is DragGestureRecognizer drag && drag.BindingContext is ITrackSymbol symbol) {
            e.Data.Properties.Add("Track", symbol);
            e.Data.Properties.Add("Source", "Symbol");
        }
    }

    //private void OnSymbolDrop(object sender, DropEventArgs e) {
    //    if (e.Data.Properties.ContainsKey("TrackSymbol")) {
    //        var trackSymbol = e.Data.Properties["Track"] as ITrackSymbol;
    //        // Handle the drop event here
    //    }
    //}
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
            trackSelectedEvent.Track?.RotateLeft();
            break;
        case 2:
            Navigation.PushModalAsync(new PanelPropertyPage(trackSelectedEvent.Track));
            break;
        }
    }
    #endregion
    
    private void SavePanelAndExit(object? sender, EventArgs e) {
        Navigation.PopAsync();
    }

    private void DeletePanelAndExit(object? sender, EventArgs e) {
        Navigation.PopAsync();
    }

    private void ToggleGrid(object? sender, EventArgs e) {
        PanelView.ShowGrid = !PanelView.ShowGrid;
        PanelView.RebuildGrid(true);
    }

    private void ShowPropertyPage(object? sender, EventArgs e) {
        // If the view model has selected items, then we do the properties on 
        // those item(s). If not, then we do the main panel page properties.
        Navigation.PushModalAsync(_selectedTrack is not null ? new PanelPropertyPage(_selectedTrack) : new PanelPropertyPage(Panel));
    }

    private void DropTrackInTrash(object? sender, DropEventArgs e) {
        e.Data.Properties.TryGetValue("Source", out var source);
        e.Data.Properties.TryGetValue("Track", out var track);
        Console.WriteLine($"Deleting track {track}");
        if (source is string && source.Equals("Panel") && track is ITrackPiece trackPiece) Panel.Tracks.Remove(trackPiece);
        DragTrashIcon.BackgroundColor = Colors.Transparent;
        PanelView.RebuildGrid(true);
    }

    private void DropTrackInTrashHoverOver(object? sender, DragEventArgs e) {
        e.Data.Properties.TryGetValue("Source", out var source);
        if (source is string && source.Equals("Panel")) DragTrashIcon.BackgroundColor = Colors.Red; 
    }

    private void DropTrackInTrashHoverLeave(object? sender, DragEventArgs e) {
        DragTrashIcon.BackgroundColor = Colors.Transparent;
    }

    #region Code to manage the Left and Right Panels
    private void OnPanUpdated(object sender, PanUpdatedEventArgs e) {
        switch (e.StatusType) {
        case GestureStatus.Started:
            _initialX = e.TotalX;
            _isDragging = true;
            LeftColumn.Width = new GridLength(LeftPane.Width, GridUnitType.Absolute);
            RightColumn.Width = new GridLength(RightPane.Width, GridUnitType.Absolute);
            Separator.Color = Colors.Gold;
            break;

        case GestureStatus.Running:
            var deltaX = e.TotalX - _initialX;
            _initialX = e.TotalX;

            // Adjust the widths of the left and right columns
            var newRightWidth = Math.Clamp(RightColumn.Width.Value - deltaX, MinRightPaneWidth, MaxRightPaneWidth);
            var newLeftWidth = Math.Max(0, MainGrid.Width - newRightWidth - Separator.WidthRequest);
            LeftColumn.Width = new GridLength(newLeftWidth, GridUnitType.Absolute);
            RightColumn.Width = new GridLength(newRightWidth, GridUnitType.Absolute);
            AdjustColumnCount();
            break;

        case GestureStatus.Completed:
        case GestureStatus.Canceled:
            _initialX = 0;
            _isDragging = false;
            Separator.Color = Colors.Gray;
            AdjustColumnCount();
            break;
        }
    }
    
    // Event handler for the SizeChanged event
    private void OnSizeChanged(object? sender, EventArgs e) {
        if (!_isDragging) {
            var rightWidth = RightColumn.Width.Value; 
            var leftWidth = Math.Max(0, MainGrid.Width - rightWidth);
            LeftColumn.Width = new GridLength(leftWidth, GridUnitType.Absolute);
        }
        AdjustColumnCount();
    }
    
    // Method to collapse or expand the right pane
    public void ToggleRightPane() {
        if (RightColumn.Width.Value == 0) {
            RightColumn.Width = new GridLength(200, GridUnitType.Absolute);
            Separator.IsVisible = true;
            ExpandCollapse.IconImageSource = "right_panel_close_filled.png";
        }
        else {
            RightColumn.Width = new GridLength(0, GridUnitType.Absolute);
            Separator.IsVisible = false;
            ExpandCollapse.IconImageSource = "right_panel_open_filled.png";
        }
        OnSizeChanged(null, EventArgs.Empty);
    }
    
    private void OnExpandCollapseButtonClicked(object sender, EventArgs e) {
        ToggleRightPane();
    }
    
    private void AdjustColumnCount() {
        if (ItemsLayout == null) return;

        if (RightPane.Width <= 150) { // Single column if the width is less than 400
            ItemsLayout.Span = 1;
        }
        else if (RightPane.Width <= 225) { // Two columns if the width is between 400 and 800
            ItemsLayout.Span = 2;
        }
        else { // Three columns if the width is greater than 800
            ItemsLayout.Span = 3;
        }
    }
    #endregion

}