using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View.ControlPanel;

public readonly record struct PlacementRect(int Col, int Row, int Width, int Height, bool InBounds, bool Collides);

public readonly record struct SelectionPlacementResult(bool CanPlace, List<PlacementRect> Cells);

/// <summary>
///     Utility class for grid position calculations and tile lookups.
///     Consolidates all position-related logic that was scattered throughout ControlPanelView.
/// </summary>
public static class GridPositionHelper {
    /// <summary>
    ///     Convert a position in the grid (absolute) to a Grid position within the col/row definitions
    /// </summary>
    /// <param name="point">A point object of where the item was tapped</param>
    /// <param name="grid">The grid to calculate position for</param>
    /// <returns>Either null, or (col, row) coordinates</returns>
    public static (int Col, int Row)? GetGridPosition(Point? point, Grid grid) {
        if (point is not { } tapPosition) return(0, 0);
        return GetGridPosition(tapPosition.X, tapPosition.Y, grid);
    }

    /// <summary>
    ///     Convert absolute X,Y coordinates to grid col/row coordinates
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

        return(col, row);
    }

    #region Tile Lookup Methods
    /// <summary>
    ///     Find all tiles at the specified grid position, ordered by layer (highest first)
    /// </summary>
    public static List<ITile> GetTilesAt(int col, int row, Grid grid) => grid.Children
                                                                             .OfType<ITile>()
                                                                             .Where(x => x.Entity.Col == col && x.Entity.Row == row)
                                                                             .OrderByDescending(x => x.Entity.Layer)
                                                                             .ToList();

    /// <summary>
    ///     Find all tiles at the specified grid position
    /// </summary>
    public static List<ITile> GetTilesAt((int col, int row) position, Grid grid) => GetTilesAt(position.col, position.row, grid);

    /// <summary>
    ///     Find all tiles that match the position of the provided tile
    /// </summary>
    public static List<ITile> GetTilesAt(ITile tile, Grid grid) => GetTilesAt(tile.Entity.Col, tile.Entity.Row, grid);

    /// <summary>
    ///     Find all interactive tiles at the specified grid position, ordered by layer (highest first)
    /// </summary>
    public static List<ITile> GetInteractiveTilesAt(int col, int row, Grid grid) => grid.Children
                                                                                        .OfType<ITile>()
                                                                                        .Where(x => x.Entity is IInteractiveEntity &&
                                                                                                    x.Entity.Col == col &&
                                                                                                    x.Entity.Row == row)
                                                                                        .OrderByDescending(x => x.Entity.Layer)
                                                                                        .ToList();

    /// <summary>
    ///     Find all interactive tiles at the specified position
    /// </summary>
    public static List<ITile> GetInteractiveTilesAt((int col, int row) position, Grid grid) => GetInteractiveTilesAt(position.col, position.row, grid);

    /// <summary>
    ///     Find all track tiles (non-interactive) at the specified grid position, ordered by layer (highest first)
    /// </summary>
    public static List<ITile> GetTrackTilesAt(int col, int row, Grid grid) => grid.Children
                                                                                  .OfType<ITile>()
                                                                                  .Where(x => x.Entity is ITrackEntity &&
                                                                                              x.Entity is not IInteractiveEntity &&
                                                                                              x.Entity.Col == col &&
                                                                                              x.Entity.Row == row)
                                                                                  .OrderByDescending(x => x.Entity.Layer)
                                                                                  .ToList();

    /// <summary>
    ///     Find all track tiles at the specified position
    /// </summary>
    public static List<ITile> GetTrackTilesAt((int col, int row) position, Grid grid) => GetTrackTilesAt(position.col, position.row, grid);

    /// <summary>
    ///     Find all tiles whose span covers the specified grid cell (col,row),
    ///     ordered by layer (highest first).
    ///     Use this when there is no exact top-left match but a tile may occupy the cell.
    /// </summary>
    public static ITile? GetTopmostTileCovering(int col, int row, Grid grid) => GetTilesCovering(col, row, grid).FirstOrDefault();

    public static List<ITile> GetTilesCovering(int col, int row, Grid grid) => grid.Children
                                                                                   .OfType<ITile>()
                                                                                   .Where(t =>
                                                                                        col >= t.Entity.Col &&
                                                                                        col < t.Entity.Col + t.Entity.Width &&
                                                                                        row >= t.Entity.Row &&
                                                                                        row < t.Entity.Row + t.Entity.Height)
                                                                                   .OrderByDescending(t => t.Entity.Layer)
                                                                                   .ToList();
    #endregion

    #region Existence Check Methods
    /// <summary>
    ///     Check if any tile exists at the specified position
    /// </summary>
    public static bool HasTileAt(int col, int row, Grid grid) => GetTilesAt(col, row, grid).Count > 0;

    /// <summary>
    ///     Check if any tile exists at the specified position
    /// </summary>
    public static bool HasTileAt((int col, int row) position, Grid grid) => HasTileAt(position.col, position.row, grid);

    /// <summary>
    ///     Check if any interactive tile exists at the specified position
    /// </summary>
    public static bool HasInteractiveTileAt(int col, int row, Grid grid) => GetInteractiveTilesAt(col, row, grid).Count > 0;

    /// <summary>
    ///     Check if any interactive tile exists at the specified position
    /// </summary>
    public static bool HasInteractiveTileAt((int col, int row) position, Grid grid) => HasInteractiveTileAt(position.col, position.row, grid);

    /// <summary>
    ///     Check if any track tile exists at the specified position
    /// </summary>
    public static bool HasTrackTileAt(int col, int row, Grid grid) => GetTrackTilesAt(col, row, grid).Count > 0;

    /// <summary>
    ///     Check if any track tile exists at the specified position
    /// </summary>
    public static bool HasTrackTileAt((int col, int row) position, Grid grid) => HasTrackTileAt(position.col, position.row, grid);
    #endregion

    #region Validation Methods
    /// <summary>
    ///     Check if the specified bounds are completely within the grid
    /// </summary>
    public static bool IsInBounds(int col, int row, int width, int height, int maxCols, int maxRows) => col >= 0 && row >= 0 &&
                                                                                                        width >= 1 && height >= 1 &&
                                                                                                        col + width <= maxCols &&
                                                                                                        row + height <= maxRows;

    /// <summary>
    ///     Check if a tile would be within bounds at the specified position
    /// </summary>
    public static bool IsInBounds(ITile tile, int col, int row, int maxCols, int maxRows) => IsInBounds(col, row, tile.Entity.Width, tile.Entity.Height, maxCols, maxRows);

    /// <summary>
    ///     Check if two rectangles overlap using axis-aligned bounding box test
    /// </summary>
    public static bool RectsOverlap(int aCol, int aRow, int aWidth, int aHeight,
        int bCol, int bRow, int bWidth, int bHeight) => aCol < bCol + bWidth && aCol + aWidth > bCol &&
                                                        aRow < bRow + bHeight && aRow + aHeight > bRow;

    public static bool WouldCollide(ITile tile, int col, int row, Grid grid, EditModeEnum mode,
        ISet<ITile>? exclude = null) // prefer passing a HashSet here if you use exclusions often
    {
        // Rule 4: drawing-only tiles never collide
        var ent = tile.Entity;
        var aIsTrack = ent is ITrackEntity;
        var aIsInteractive = ent is IInteractiveEntity;
        if (!aIsTrack && !aIsInteractive) return false;

        // Can't drop onto exact same top-left (unless Move mode)
        if (ent.Col == col && ent.Row == row && mode != EditModeEnum.Move) return true;

        // Proposed destination rect (integer grid cells)
        int aCol = col, aRow = row;
        int aW = ent.Width, aH = ent.Height;
        var aRight = aCol + aW;
        var aBottom = aRow + aH;

        // Iterate children without LINQ; bail early on first disallowed overlap
        var children = grid.Children;
        for (int i = 0, n = children.Count; i < n; i++) {
            if (children[i] is not ITile other) continue;
            if (ReferenceEquals(other, tile)) continue;
            if (exclude is { } && exclude.Contains(other)) continue;

            var bEnt = other.Entity;

            // Skip drawing-only targets (rule 4) early
            // (Don't compute types until we know they overlap)
            int bCol = bEnt.Col, bRow = bEnt.Row;
            int bW = bEnt.Width, bH = bEnt.Height;

            // Fast AABB overlap in grid space
            // !(aRight <= bCol || aCol >= bCol + bW || aBottom <= bRow || aRow >= bRow + bH)
            if (aRight <= bCol || aCol >= bCol + bW || aBottom <= bRow || aRow >= bRow + bH) {
                continue;
            }

            // Now that we know they overlap, check types
            var bIsTrack = bEnt is ITrackEntity;
            var bIsInteractive = bEnt is IInteractiveEntity;
            if (!bIsTrack && !bIsInteractive) continue; // drawing-only target, allowed

            // Rule 1: Track vs Track not allowed
            if (aIsTrack && bIsTrack) return true;

            // Rule 2: Interactive vs Interactive not allowed
            if (aIsInteractive && bIsInteractive) return true;

            // Rule 3: Track + Interactive is allowed → keep scanning
        }
        return false;
    }

    public static bool WouldCollide(ITile tile, int col, int row, Grid grid, EditModeEnum mode, IEnumerable<ITile>? excludeTiles) {
        // Avoid per-call HashSet when possible
        var set = excludeTiles as ISet<ITile>;
        set ??= excludeTiles is null ? null : new HashSet<ITile>(excludeTiles);
        return WouldCollide(tile, col, row, grid, mode, set);
    }

    public static SelectionPlacementResult EvaluateSelectionPlacement(IReadOnlyCollection<ITile> selection,
        int anchorCol,
        int anchorRow,
        Grid grid,
        EditModeEnum mode,
        int maxCols,
        int maxRows) {
        // Empty selection -> trivially placeable
        if (selection is null || selection.Count == 0) {
            return new SelectionPlacementResult(true, new List<PlacementRect>(0));
        }

        // Find selection's current top-left to preserve relative offsets
        int minCol = int.MaxValue, minRow = int.MaxValue;
        foreach (var t in selection) {
            var e = t.Entity;
            if (e.Col < minCol) minCol = e.Col;
            if (e.Row < minRow) minRow = e.Row;
        }

        // When moving, ignore collisions against tiles that are themselves moving away
        var exclude = mode == EditModeEnum.Move
            ? selection as ISet<ITile> ?? new HashSet<ITile>(selection)
            : null;

        var cells = new List<PlacementRect>(selection.Count);
        var allOk = true;

        foreach (var tile in selection) {
            var e = tile.Entity;

            // Destination for this tile keeping its offset within the selection
            var destCol = anchorCol + (e.Col - minCol);
            var destRow = anchorRow + (e.Row - minRow);

            var inBounds = IsInBounds(destCol, destRow, e.Width, e.Height, maxCols, maxRows);
            var collides = false;

            if (inBounds) {
                // Reuse the fast per-tile rule set (track/interactive/drawing + AABB)
                collides = WouldCollide(tile, destCol, destRow, grid, mode, exclude);
            }

            cells.Add(new PlacementRect(destCol, destRow, e.Width, e.Height, inBounds, collides));
            if (allOk && (!inBounds || collides)) allOk = false;
        }

        return new SelectionPlacementResult(allOk, cells);
    }

    /// <summary>
    ///     Calculate the optimal grid size based on available space and grid dimensions
    /// </summary>
    public static double CalculateOptimalGridSize(double availableWidth, double availableHeight, int cols, int rows) {
        if (availableWidth <= 0 || availableHeight <= 0 || cols <= 0 || rows <= 0) return 1;

        var gridSize = Math.Min(availableWidth / cols, availableHeight / rows);

        // Round down to nearest hundredth for consistent sizing
        return Math.Floor(gridSize * 100) / 100.0;
    }

    /// <summary>
    ///     Get all tiles that would be affected by a selection rectangle
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
    ///     Find all views in the grid with the specified ClassId
    /// </summary>
    public static List<IView> GetViewsByClassId(Grid grid, string classId) => grid.Children
                                                                                  .Where(x => x is Microsoft.Maui.Controls.View view && view.ClassId == classId)
                                                                                  .ToList();

    /// <summary>
    ///     Remove all views with the specified ClassId from the grid
    /// </summary>
    public static void RemoveViewsByClassId(Grid grid, string classId) {
        var viewsToRemove = GetViewsByClassId(grid, classId);
        foreach (var view in viewsToRemove) {
            grid.Children.Remove(view);
        }
    }

    /// <summary>
    ///     Set the grid position properties for a tile
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