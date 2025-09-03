using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.View.ControlPanel;

public enum CellHighlightAction { Selected, DragInvalid, DragValid, Resize, Selecting, Error }

public class GridHighlightOutline : GraphicsView, IDrawable {
    public int Col { get; set; }
    public int Row { get; set; }
    public int OutlineWidth { get; set; }
    public int OutlineHeight { get; set; }
    public double CellSize { get; set; }
    public CellHighlightAction Action { get; set; }
    
    public void Draw(ICanvas canvas, RectF dirtyRect) {
        var left    = (float)(Col * CellSize);
        var top     = (float)(Row * CellSize);
        var width   = (float)(OutlineWidth  * CellSize);
        var height  = (float)(OutlineHeight * CellSize);

        canvas.FillColor = FillColor;
        canvas.FillRectangle(left, top, width, height);

        canvas.StrokeSize = 3;
        canvas.StrokeColor = OutlineColor;
        canvas.DrawRectangle(left, top, width, height);
    }

    public GridHighlightOutline(int col, int row, int width, int height, double cellSize, CellHighlightAction action) {
        SetBounds(col, row, width, height, cellSize, action);
        InputTransparent = true;
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
        ClassId = "Highlight";
        Drawable = this;
    }
    
    public void SetBounds(int col, int row, int width, int height, double cellSize, CellHighlightAction action) {
        Col     = col;
        Row      = row;
        OutlineWidth    = width;
        OutlineHeight   = height;
        CellSize        = cellSize;
        Action          = action;
        Invalidate();
    }

    private Color FillColor => OutlineColor.WithAlpha(0.25f);
    private Color OutlineColor =>
        Action switch {
            CellHighlightAction.Selected    => Colors.CornflowerBlue,
            CellHighlightAction.Resize      => Colors.MidnightBlue,
            CellHighlightAction.DragValid   => Colors.CornflowerBlue,
            CellHighlightAction.DragInvalid => Colors.Red,
            CellHighlightAction.Selecting   => Colors.LightSkyBlue,
            _                               => Colors.Red
        };
}