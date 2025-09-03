namespace DCCPanelController.View.Helpers;

/// <summary>
///     This is a helper class that draws the Grid Lines on the Page.
/// </summary>
/// <param name="rows">Number of rows to Draw</param>
/// <param name="columns">Number of cols to Draw</param>
public class GridLinesDrawable(int rows, int columns, Color? gridColor = null, float? lineWidth = null, float? gridWidth = null) : IDrawable {
    private Color GridColor { get; } = gridColor ?? Colors.DarkGrey;
    private float LineWidth { get; } = lineWidth ?? 0.5f;
    private float GridWidth { get; } = gridWidth ?? 5.0f;

    public void Draw(ICanvas canvas, RectF dirtyRect) {
        var cellWidth = dirtyRect.Width / columns;
        var cellHeight = dirtyRect.Height / rows;
        canvas.StrokeColor = GridColor;

        for (var i = 0; i <= rows; i++) {
            canvas.StrokeSize = i == 0 || i == rows ? GridWidth : LineWidth;
            canvas.DrawLine(0, i * cellHeight, dirtyRect.Width, i * cellHeight);
        }

        for (var j = 0; j <= columns; j++) {
            canvas.StrokeSize = j == 0 || j == columns ? GridWidth : LineWidth;
            canvas.DrawLine(j * cellWidth, 0, j * cellWidth, dirtyRect.Height);
        }
    }
}