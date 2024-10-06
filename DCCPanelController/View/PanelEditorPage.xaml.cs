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
using DCCPanelController.ViewModel;
using Color = System.Drawing.Color;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {

    private const double MinRightPaneWidth = 100; // Minimum width constraint for the right pane
    private const double MaxRightPaneWidth = 300; // Maximum width constraint for the right pane

    private bool _multiSelect = false;
    private readonly List<ITrackPiece> _selectedTracks = [];
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
    }

    #region Handle Selecting and Actioning on a Track including multiple selections
    /// <summary>
    /// When a TrackPiece is selected, track it and keep it and tell the
    /// underlying panel to highlight it. This is to support future MULTI-SELECT
    /// mode. 
    /// </summary>
    private void OnTrackPieceTapped(object? sender, ITrackPiece e) {
        if (_multiSelect)  HandleMultiSelect(e);
        if (!_multiSelect) HandleSingleSelect(e);
        RotateButton.IsEnabled = _selectedTracks.Count > 0;
    }

    private void HandleMultiSelect(ITrackPiece e) {
        var isTrackSelected = _selectedTracks.Contains(e);
        if (isTrackSelected) UnHighlightAndRemoveTrack(e);
        if (!isTrackSelected) HighlightAndAddTrack(e);
    }

    private void HandleSingleSelect(ITrackPiece e) {
        foreach (var track in _selectedTracks) {
            UnHighlightTrack(track);
        }
        _selectedTracks.Clear();
        HighlightAndAddTrack(e);
    }

    private void HighlightAndAddTrack(ITrackPiece track) {
        _selectedTracks.Add(track);
        PanelView.HighlightCell(track.X, track.Y);
    }

    private void UnHighlightAndRemoveTracks() {
        foreach ( var track in _selectedTracks) UnHighlightAndRemoveTrack(track);
    }

    private void UnHighlightAndRemoveTrack(ITrackPiece track) {
        PanelView.UnHighlightCell(track.X, track.Y);
        _selectedTracks.Remove(track);
    }

    private void UnHighlightTrack(ITrackPiece track) {
        PanelView.UnHighlightCell(track.X, track.Y);
    }
    
    private void ToggleMultiSelect(object? sender, EventArgs e) {
        _multiSelect = !_multiSelect;
        if (_multiSelect == false) UnHighlightAndRemoveTracks();
        SelectButton.IconImageSource = _multiSelect ? "select.png" : "deselect.png";
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
    }

    private void ShowPropertyPage(object? sender, EventArgs e) {
        // If the view model has selected items, then we do the properties on 
        // those item(s). If not, then we do the main panel page properties. 
        Navigation.PushModalAsync(new PanelPropertyPage(Panel));
    }

    private void RotateCurrentItem(object? sender, EventArgs e) {
        foreach (var track in _selectedTracks) {
            track.RotateLeft();
        }
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
            break;

        case GestureStatus.Completed:
        case GestureStatus.Canceled:
            _initialX = 0;
            _isDragging = false;
            Separator.Color = Colors.Gray;
            break;
        }
    }
    
    // Event handler for the SizeChanged event
    private void OnSizeChanged(object? sender, EventArgs e) {
        if (!_isDragging) {
            var rightWidth = RightColumn.Width.Value; 
            //var leftWidth = Math.Max(0, MainGrid.Width - rightWidth - (Separator.IsVisible ? Separator.WidthRequest : 0));
            var leftWidth = Math.Max(0, MainGrid.Width - rightWidth);
            LeftColumn.Width = new GridLength(leftWidth, GridUnitType.Absolute);
        }
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
    #endregion

}