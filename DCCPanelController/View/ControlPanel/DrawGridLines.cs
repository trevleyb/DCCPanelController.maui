namespace DCCPanelController.View.ControlPanel;

/// <summary>
///     This is a helper class that draws the Grid Lines on the Page.
/// </summary>
public class DrawGridLines : GraphicsView, IDrawable {
    /// <summary>
    ///     This is a helper class that draws the Grid Lines on the Page.
    /// </summary>
    public DrawGridLines(int? columns = null, int? rows = null, Color? gridColor = null, float? lineWidth = null, float? gridWidth = null) {
        Rows = rows ?? 0;
        Cols = columns ?? 0;
        GridColor = gridColor ?? Colors.DarkGrey;
        LineWidth = lineWidth ?? 1.0f;
        GridWidth = gridWidth ?? 5.0f;

        ClassId = "GridLines";
        InputTransparent = true;
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
        ZIndex = 1;
        IsActive = false;
        Drawable = this;
    }

    private int Cols { get; set; }
    private int Rows { get; set; }

    private float GridWidth { get; set; }
    private float LineWidth { get; set; }
    private Color GridColor { get; set; }

    public bool IsActive {
        get;
        set {
            field = value;
            Invalidate();
        }
    }

    public void Draw(ICanvas canvas, RectF dirtyRect) {
        if (IsActive && Cols > 0 && Rows > 0) {
            IsVisible = true;

            var cellWidth = dirtyRect.Width / Cols;
            var cellHeight = dirtyRect.Height / Rows;
            canvas.StrokeColor = GridColor;

            for (var i = 0; i <= Rows; i++) {
                canvas.StrokeSize = i == 0 || i == Rows ? GridWidth : LineWidth;
                canvas.DrawLine(0, i * cellHeight, dirtyRect.Width, i * cellHeight);
            }

            for (var j = 0; j <= Cols; j++) {
                canvas.StrokeSize = j == 0 || j == Cols ? GridWidth : LineWidth;
                canvas.DrawLine(j * cellWidth, 0, j * cellWidth, dirtyRect.Height);
            }
        } else {
            IsVisible = false;
        }
    }

    public void Update(int columns, int rows, Color? gridColor = null, float? lineWidth = null, float? gridWidth = null) {
        Rows = rows;
        Cols = columns;
        GridColor = gridColor ?? Colors.DarkGrey;
        LineWidth = lineWidth ?? 0.5f;
        GridWidth = gridWidth ?? 5.0f;
        Invalidate();
    }
}