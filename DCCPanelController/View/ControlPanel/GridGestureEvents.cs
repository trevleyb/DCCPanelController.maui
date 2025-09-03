using System.ComponentModel;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;
using DCCPanelController.Models.ViewModel.Interfaces;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Helpers;

/// <summary>
/// Event arguments for basic grid gesture events (tap, long press)
/// </summary>
public class GridGestureEventArgs : EventArgs {
    public object? Sender { get; }
    public int Col { get; }
    public int Row { get; }
    public int TapCount { get; set; }

    public GridGestureEventArgs(object? sender, int col, int row) {
        Sender = sender;
        Col = col;
        Row = row;
    }
}

/// <summary>
/// Event arguments for tile drag operations
/// </summary>
public class TileDragEventArgs : EventArgs {
    public ITile Tile { get; }
    public int StartCol { get; }
    public int StartRow { get; }
    public int CurrentCol { get; }
    public int CurrentRow { get; }

    public TileDragEventArgs(ITile tile, int startCol, int startRow, int currentCol, int currentRow) {
        Tile = tile;
        StartCol = startCol;
        StartRow = startRow;
        CurrentCol = currentCol;
        CurrentRow = currentRow;
    }
}

/// <summary>
/// Event arguments for grid selection operations
/// </summary>
public class GridSelectionEventArgs : EventArgs {
    public Point? StartPosition { get; }
    public Point? CurrentPosition { get; }
    public int Col { get; }
    public int Row { get; }

    public GridSelectionEventArgs(int col, int row) {
        Col = col;
        Row = row;
    }

    public GridSelectionEventArgs(Point? startPos, Point? currentPos, int col, int row) {
        StartPosition = startPos;
        CurrentPosition = currentPos;
        Col = col;
        Row = row;
    }
}