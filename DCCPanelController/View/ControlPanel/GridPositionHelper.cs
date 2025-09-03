using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View.ControlPanel;

/// <summary>
/// Utility class for grid position calculations and tile lookups.
/// Consolidates all position-related logic that was scattered throughout ControlPanelView.
/// </summary>
public static class GridPositionHelper {
    /// <summary>
    /// Convert a position in the grid (absolute) to a Grid position within the col/row definitions
    /// </summary>
    /// <param name="point">A point object of where the item was tapped</param>
    /// <param name="grid">The grid to calculate position for</param>
    /// <returns>Either null, or (col, row) coordinates</returns>
    public static (int Col, int Row)? GetGridPosition(Point? point, Grid grid) {
        if (point is not { } tapPosition) return (0, 0);
        return GetGridPosition(tapPosition.X, tapPosition.Y, grid);
    }

    /// <summary>
    /// Convert absolute X,Y coordinates to grid col/row coordinates
    /// </summary>
    public static (int Col, int Row)? GetGridPosition(double posX, double posY, Grid grid) {
        var totalHeight = grid.Height;
        var totalWidth = grid.Width;
        var rowCount = grid.RowDefinitions.Count;
        var colCount = grid.ColumnDefinitions.Count;

        if (rowCount == 0 || colCount == 0) return null;

        var cellHeight = totalHeight / rowCount;
        var cellWidth = totalWidth / colCount;

        if (cellHeight <= 0 || cellWidth <= 0) return null;

        // Calculate row and column indices
        var row = (int)(posY / cellHeight);
        var col = (int)(posX / cellWidth);

        // Ensure indices are within bounds
        row = Math.Min(Math.Max(0, row), rowCount - 1);
        col = Math.Min(Math.Max(0, col), colCount - 1);

        return (col, row);
    }

    #region Tile Lookup Methods
    /// <summary>
    /// Find all tiles at the specified grid position, ordered by layer (highest first)
    /// </summary>
    public static List<ITile> GetTilesAt(int col, int row, Grid grid) {
        return grid.Children
                   .OfType<ITile>()
                   .Where(x => x.Entity.Col == col && x.Entity.Row == row)
                   .OrderByDescending(x => x.Entity.Layer)
                   .ToList();
    }

    /// <summary>
    /// Find all tiles at the specified grid position
    /// </summary>
    public static List<ITile> GetTilesAt((int col, int row) position, Grid grid) => GetTilesAt(position.col, position.row, grid);

    /// <summary>
    /// Find all tiles that match the position of the provided tile
    /// </summary>
    public static List<ITile> GetTilesAt(ITile tile, Grid grid) => GetTilesAt(tile.Entity.Col, tile.Entity.Row, grid);

    /// <summary>
    /// Find all interactive tiles at the specified grid position, ordered by layer (highest first)
    /// </summary>
    public static List<ITile> GetInteractiveTilesAt(int col, int row, Grid grid) {
        return grid.Children
                   .OfType<ITile>()
                   .Where(x => x.Entity is IInteractiveEntity &&
                               x.Entity.Col == col &&
                               x.Entity.Row == row)
                   .OrderByDescending(x => x.Entity.Layer)
                   .ToList();
    }

    /// <summary>
    /// Find all interactive tiles at the specified position
    /// </summary>
    public static List<ITile> GetInteractiveTilesAt((int col, int row) position, Grid grid) => GetInteractiveTilesAt(position.col, position.row, grid);

    /// <summary>
    /// Find all track tiles (non-interactive) at the specified grid position, ordered by layer (highest first)
    /// </summary>
    public static List<ITile> GetTrackTilesAt(int col, int row, Grid grid) {
        return grid.Children
                   .OfType<ITile>()
                   .Where(x => x.Entity is ITrackEntity &&
                               x.Entity is not IInteractiveEntity &&
                               x.Entity.Col == col &&
                               x.Entity.Row == row)
                   .OrderByDescending(x => x.Entity.Layer)
                   .ToList();
    }

    /// <summary>
    /// Find all track tiles at the specified position
    /// </summary>
    public static List<ITile> GetTrackTilesAt((int col, int row) position, Grid grid) => GetTrackTilesAt(position.col, position.row, grid);
    #endregion

    #region Existence Check Methods
    /// <summary>
    /// Check if any tile exists at the specified position
    /// </summary>
    public static bool HasTileAt(int col, int row, Grid grid) => GetTilesAt(col, row, grid).Count > 0;

    /// <summary>
    /// Check if any tile exists at the specified position
    /// </summary>
    public static bool HasTileAt((int col, int row) position, Grid grid) => HasTileAt(position.col, position.row, grid);

    /// <summary>
    /// Check if any interactive tile exists at the specified position
    /// </summary>
    public static bool HasInteractiveTileAt(int col, int row, Grid grid) => GetInteractiveTilesAt(col, row, grid).Count > 0;

    /// <summary>
    /// Check if any interactive tile exists at the specified position
    /// </summary>
    public static bool HasInteractiveTileAt((int col, int row) position, Grid grid) => HasInteractiveTileAt(position.col, position.row, grid);

    /// <summary>
    /// Check if any track tile exists at the specified position
    /// </summary>
    public static bool HasTrackTileAt(int col, int row, Grid grid) => GetTrackTilesAt(col, row, grid).Count > 0;

    /// <summary>
    /// Check if any track tile exists at the specified position
    /// </summary>
    public static bool HasTrackTileAt((int col, int row) position, Grid grid) => HasTrackTileAt(position.col, position.row, grid);
    #endregion

    #region Validation Methods
    /// <summary>
    /// Check if the specified bounds are completely within the grid
    /// </summary>
    public static bool IsInBounds(int col, int row, int width, int height, int maxCols, int maxRows) {
        return col >= 0 && row >= 0 &&
               width >= 1 && height >= 1 &&
               col + width <= maxCols &&
               row + height <= maxRows;
    }

    /// <summary>
    /// Check if a tile would be within bounds at the specified position
    /// </summary>
    public static bool IsInBounds(ITile tile, int col, int row, int maxCols, int maxRows) {
        return IsInBounds(col, row, tile.Entity.Width, tile.Entity.Height, maxCols, maxRows);
    }

    /// <summary>
    /// Check if two rectangles overlap using axis-aligned bounding box test
    /// </summary>
    public static bool RectsOverlap(int aCol, int aRow, int aWidth, int aHeight,
                                    int bCol, int bRow, int bWidth, int bHeight) {
        return aCol < bCol + bWidth && aCol + aWidth > bCol &&
               aRow < bRow + bHeight && aRow + aHeight > bRow;
    }

    /// <summary>
    /// Check if placing a tile at the specified position would cause a collision
    /// </summary>
    /// <param name="tile">The tile to check</param>
    /// <param name="col">Destination column</param>
    /// <param name="row">Destination row</param>
    /// <param name="grid">The grid to check against</param>
    /// <param name="mode">Edit mode (affects collision rules)</param>
    /// <param name="excludeTiles">Tiles to exclude from collision check (e.g., currently selected tiles in move mode)</param>
    /// <returns>True if there would be a collision</returns>
    public static bool WouldCollide(ITile tile, int col, int row, Grid grid,
                                    EditModeEnum mode, IEnumerable<ITile>? excludeTiles = null) {
        
        // Non-track entities don't have collision restrictions
        // Just can't have more than 1 Track Entity in the same grid location 
        // or more than 1 interactive tile in the same grid location
        // ----------------------------------------------------------------------
        if (tile.Entity is not ITrackEntity) return false;

        // Can't drop onto yourself (unless in Move mode where you're vacating the space)
        // ------------------------------------------------------------------------------
        if (tile.Entity.Col == col && tile.Entity.Row == row && mode != EditModeEnum.Move) return true;

        var exclusionSet = excludeTiles?.ToHashSet() ?? [];
        var conflictingTiles = grid.Children.OfType<ITile>()
            .Where(other => other != tile &&
                !exclusionSet.Contains(other) &&
                other.Entity is ITrackEntity &&
                RectsOverlap(col, row, tile.Entity.Width, tile.Entity.Height,
                    other.Entity.Col, other.Entity.Row, other.Entity.Width, other.Entity.Height))
                .ToList();

        // If there are no conflicts, no collision
        // --------------------------------------------------
        if (conflictingTiles.Count == 0) return false;
        
        // If there are conflicts and this tile is interactive, check if any conflicting tiles are also interactive
        // (Cannot have 2 interactive tiles in the same location)
        // ------------------------------------------------------------------------------------
        return conflictingTiles.Count != 0;
    }

    /// <summary>
    /// Calculate the optimal grid size based on available space and grid dimensions
    /// </summary>
    public static double CalculateOptimalGridSize(double availableWidth, double availableHeight, int cols, int rows) {
        if (availableWidth <= 0 || availableHeight <= 0 || cols <= 0 || rows <= 0) return 1;

        var gridSize = Math.Min(availableWidth / cols, availableHeight / rows);

        // Round down to nearest hundredth for consistent sizing
        return Math.Floor(gridSize * 100) / 100.0;
    }

    /// <summary>
    /// Get all tiles that would be affected by a selection rectangle
    /// </summary>
    public static List<ITile> GetTilesInSelection(int startCol, int startRow, int endCol, int endRow, Grid grid) {
        var minCol = Math.Min(startCol, endCol);
        var maxCol = Math.Max(startCol, endCol);
        var minRow = Math.Min(startRow, endRow);
        var maxRow = Math.Max(startRow, endRow);

        return grid.Children
                   .OfType<ITile>()
                   .Where(tile => tile.Entity.Col >= minCol && tile.Entity.Col <= maxCol &&
                                  tile.Entity.Row >= minRow && tile.Entity.Row <= maxRow)
                   .ToList();
    }
    #endregion

    #region Grid Management Utilities
    /// <summary>
    /// Find all views in the grid with the specified ClassId
    /// </summary>
    public static List<IView> GetViewsByClassId(Grid grid, string classId) {
        return grid.Children
                   .Where(x => x is Microsoft.Maui.Controls.View view && view.ClassId == classId)
                   .ToList();
    }

    /// <summary>
    /// Remove all views with the specified ClassId from the grid
    /// </summary>
    public static void RemoveViewsByClassId(Grid grid, string classId) {
        var viewsToRemove = GetViewsByClassId(grid, classId);
        foreach (var view in viewsToRemove) {
            grid.Children.Remove(view);
        }
    }

    /// <summary>
    /// Set the grid position properties for a tile
    /// </summary>
    public static void SetTileGridPosition(ITile tile, Grid grid) {
        if (tile is ContentView view) {
            Grid.SetColumn(view, tile.Entity.Col);
            Grid.SetRow(view, tile.Entity.Row);
            Grid.SetColumnSpan(view, tile.Entity.Width);
            Grid.SetRowSpan(view, tile.Entity.Height);
        }
    }
    #endregion
}