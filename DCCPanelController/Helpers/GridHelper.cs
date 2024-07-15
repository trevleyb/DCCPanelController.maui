using DCCPanelController.Model;

namespace DCCPanelController.Helpers;

public class GridHelper(int viewWidth, int viewHeight, int? panelCols = 24, int? panelRows = 18) {

    public GridHelper(double viewWidth, double viewHeight, int? panelCols = 24, int? panelRows = 18) 
        : this ((int)viewWidth, (int)viewHeight, panelCols, panelRows) {}

    /// <summary>
    /// View width and height are the width and height of the original View Panel
    /// </summary>
    public int ViewWidth    { get; init; } = viewWidth;
    public int ViewHeight   { get; init; } = viewHeight;

    /// <summary>
    /// Panel Cols and Rows are the number of Rows and Cols that we have on our view pane 
    /// </summary>
    public int PanelCols    { get; set; } = panelCols ?? 24;
    public int PanelRows    { get; set; } = panelRows ?? 18;
    
    /// <summary>
    /// Panel width and height are the total size of the viewable area excluding any margins
    /// </summary>
    public int PanelWidth  => BoxSize * PanelCols;
    public int PanelHeight => BoxSize * PanelRows;
    
    /// <summary>
    /// Margins are the left and top margins that allo us to center the view pane in the window 
    /// </summary>
    public int XMargin  => (ViewWidth - PanelWidth) / 2;
    public int YMargin  => (ViewHeight - PanelHeight) / 2;

    /// <summary>
    /// Box size is the size of the box so that it is always a square height/width box
    /// </summary>
    public int BoxSize     => (Math.Min(ViewWidth / PanelCols, ViewHeight / PanelRows) / 2) * 2; 
    
    // Helper function: Converts a grid reference (x,y) into a Grid Reference 'Xnn' and 
    // then uses that to return the center point of the location. 
    // ------------------------------------------------------------------------------------------
    public GridData GetGridCoordinates(Coordinate coordinate) {
        return GetGridCoordinatesFromReference(coordinate);
    }

    public GridData GetGridCoordinates(int x, int y) {
        var reference = GetGridReference(x, y);
        return GetGridCoordinatesFromReference(reference);
    }

    public GridData GetGridCoordinatesFromReference(Coordinate coordinates) {

        // Calculate the center coordinates of the specified cell
        // Assume everything is 1 offset not 0
        var topLeftX = (coordinates.Col -1) * BoxSize;
        var topLeftY = (coordinates.Row -1) * BoxSize;
        var centerX  = topLeftX + (BoxSize / 2);
        var centerY  = topLeftY + (BoxSize / 2);
        return new GridData(topLeftX, topLeftY, centerX, centerY, XMargin, YMargin, BoxSize); 
    }

    /// <summary>
    ///1 Looks at an X,Y location and works out which cells these would fall into 
    /// to determine the Coordinates as a Xnn format (ie: B12) 
    /// </summary>
    public Coordinate GetGridReference((int x, int y) loc) => GetGridReference(loc.x, loc.y);
    public Coordinate GetGridReference(int x, int y) {

        if (PanelWidth == 0 || PanelHeight == 0) return new Coordinate(-1, -1);

        var colIndex = (int)(x / BoxSize) + 1; // Move to 1 offset not 0 
        var rowIndex = (int)(y / BoxSize) + 1; // Move to 1 offset not 0

        if (colIndex <= 0) colIndex = 1;
        if (colIndex >= PanelCols) colIndex = PanelCols;
        if (rowIndex <= 0) rowIndex = 1;
        if (rowIndex >= PanelRows) rowIndex = PanelRows;
        
        return new Coordinate(colIndex, rowIndex);
    }
}