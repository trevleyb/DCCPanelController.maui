namespace DCCPanelController.Views.Helpers;

public class SelectionOutlineDrawable : IDrawable {
    public bool IsActive { get; set; }
    public int StartCol { get; set; }
    public int StartRow { get; set; }
    public int EndCol { get; set; }
    public int EndRow { get; set; }
    public double CellWidth { get; set; }
    public double CellHeight { get; set; }
    public Color OutlineColor { get; set; } = Colors.LightBlue;

    public void Draw(ICanvas canvas, RectF dirtyRect) {
        var left = (float)(Math.Min(StartCol, EndCol) * CellWidth);
        var top = (float)(Math.Min(StartRow, EndRow) * CellHeight);
        var width = (float)((Math.Abs(EndCol - StartCol) + 1) * CellWidth);
        var height = (float)((Math.Abs(EndRow - StartRow) + 1) * CellHeight);

        // Fill first (alpha ~0.25=25%)
        var fillColor = OutlineColor.WithAlpha(0.25f);
        canvas.FillColor = fillColor;
        canvas.FillRectangle(left, top, width, height);

        // Border
        canvas.StrokeColor = OutlineColor;
        canvas.StrokeSize = 3;
        canvas.StrokeDashPattern = [2, 2];
        canvas.DrawRectangle(left, top, width, height);
    }

    public void SetBounds(int startCol, int startRow, int endCol, int endRow, double cellSize) {
        StartCol = startCol;
        StartRow = startRow;
        EndCol = endCol;
        EndRow = endRow;
        CellWidth = cellSize;
        CellHeight = cellSize;
    }
}