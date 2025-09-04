namespace DCCPanelController.View.ControlPanel;

public class DrawGridSelection : GraphicsView, IDrawable {
    private int StartCol { get; set; }
    private int StartRow { get; set; }
    private int EndCol { get; set; }
    private int EndRow { get; set; }
    private double CellWidth { get; set; }
    private double CellHeight { get; set; }

    public bool IsActive {
        get;
        set {
            field = value;
            Invalidate();
        }
    }

    public DrawGridSelection(Color? outlineColor = null) {
        InputTransparent = true;
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
        OutlineColor = outlineColor ?? Colors.LightBlue;
        ClassId = "GridSelection";
        ZIndex = 2;
        Drawable = this;
        IsActive = false;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect) {
        if (IsActive) {
            IsVisible = true;
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
        } else {
            IsVisible = false;
        }
    }

    public Color OutlineColor {
        get;
        set {
            field = value;
            Invalidate();
        }
    }

    public void Update(int startCol, int startRow, int endCol, int endRow, double cellSize) {
        StartCol = startCol;
        StartRow = startRow;
        EndCol = endCol;
        EndRow = endRow;
        CellWidth = cellSize;
        CellHeight = cellSize;
        Invalidate();
    }
}