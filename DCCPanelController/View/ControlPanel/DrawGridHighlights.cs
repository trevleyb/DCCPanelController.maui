using DCCPanelController.Models.ViewModel.Interfaces;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.View.ControlPanel;

public enum CellHighlightAction { Selected, DragInvalid, DragValid, Resize, Selecting, Error }

public record CellHighlight(int Col, int Row, int Width, int Height, CellHighlightAction Action);

public class DrawGridHighlights : GraphicsView, IDrawable {
    
    public bool IsActive { get; set; }
    private readonly Dictionary<Guid, CellHighlight> _bounded = new();
    private readonly Dictionary<(int col, int row), CellHighlight> _unbounded = [];
    
    public double CellSize {
        get;
        set { field = value; Invalidate(); }
    }

    public DrawGridHighlights() {
        InputTransparent = true;
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
        ClassId = "GridHighlights";
        ZIndex = 3;
        Drawable = this;
    }

    // Add an Unbounded item to the list (like a destination grid)
    // ---------------------------------------------------------------------------------
    public void Add(int col, int row, int width, int height, CellHighlightAction action) {
        if (!IsActive) return;
        _unbounded.Remove((col,row));
        _unbounded.Add((col,row), new CellHighlight(col, row, width, height, action));
        Invalidate();
    }
    
    // Add a known tile to the grid
    // ---------------------------------------------------------------------------------
    public void Add(ITile tile, CellHighlightAction action) {
        if (!IsActive) return;
        if (tile is { Entity: { } entity} ) {
            _bounded.Remove(tile.Guid);
            _bounded.Add(tile.Guid, new CellHighlight(entity.Col, entity.Row, entity.Width, entity.Height, action));
            Invalidate();
        }
    }

    // Remove an unknown/unbounded item from the grid
    // ---------------------------------------------------------------------------------
    public void Remove(int col, int row) {
        if (!IsActive) return;
        if (_unbounded.Remove((col,row))) Invalidate();
    }

    // Remove a Tile from the Grid
    // ---------------------------------------------------------------------------------
    public void Remove(ITile tile) {
        if (!IsActive) return;
        if (_bounded.Remove(tile.Guid)) Invalidate();
    }

    // Remove all items from the grid
    // ---------------------------------------------------------------------------------
    public void Clear() {
        _bounded.Clear();
        _unbounded.Clear();
        Invalidate();
    }
    
    public void Draw(ICanvas canvas, RectF dirtyRect) {
        if (!IsActive || CellSize == 0) return;
        var highlights = _unbounded.Values.Union(_bounded.Values).OrderBy(x => x.Col).ThenBy(x => x.Row).ToList();
        if (highlights.Count == 0) return;

        foreach (var cell in highlights) {
            if (cell.Width == 0 || cell.Height == 0) continue;
            
            var left = (float)  (cell.Col * CellSize);
            var top = (float)   (cell.Row * CellSize);
            var width = (float) (cell.Width * CellSize);
            var height = (float)(cell.Height * CellSize);

            canvas.FillColor = FillColor(cell.Action);
            canvas.FillRectangle(left, top, width, height);

            canvas.StrokeSize = 3;
            canvas.StrokeColor = OutlineColor(cell.Action);
            canvas.DrawRectangle(left, top, width, height);
        }
    }

    private static Color FillColor(CellHighlightAction action) => OutlineColor(action).WithAlpha(0.25f);
    private static Color OutlineColor(CellHighlightAction action) =>
        action switch {
            CellHighlightAction.Selected    => Colors.CornflowerBlue,
            CellHighlightAction.Resize      => Colors.MidnightBlue,
            CellHighlightAction.DragValid   => Colors.CornflowerBlue,
            CellHighlightAction.DragInvalid => Colors.Red,
            CellHighlightAction.Selecting   => Colors.LightSkyBlue,
            _                               => Colors.Red
        };
}