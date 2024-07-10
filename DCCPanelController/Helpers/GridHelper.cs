namespace DCCPanelController.Helpers;

public class GridHelper(int viewWidth, int viewHeight, int? panelCols = 24, int? panelRows = 18) {

    public GridHelper(double viewWidth, double viewHeight, int? panelCols = 24, int? panelRows = 18) 
        : this ((int)viewWidth, (int)viewHeight, panelCols, panelRows) {}

    public int ViewWidth    { get; init; } = viewWidth;
    public int ViewHeight   { get; init; } = viewHeight;
    public int PanelCols    { get; init; } = panelCols ?? 24;
    public int PanelRows    { get; init; } = panelRows ?? 18;

    public int BoxSize     => (Math.Min(ViewWidth / PanelCols, ViewHeight / PanelRows) / 2) * 2; 
    public int PanelWidth  => BoxSize * PanelCols;
    public int PanelHeight => BoxSize * PanelRows;
    
    public int HorizontalMargin => (ViewWidth - PanelWidth) / 2;
    public int VerticalMargin   => (ViewHeight - PanelHeight) / 2;
    
    // Helper function: Converts a grid reference (x,y) into a Grid Reference 'Xnn' and 
    // then uses that to return the center point of the location. 
    // ------------------------------------------------------------------------------------------
    public GridData GetGridCoordinates(string gridReference) => GetGridCoordinatesFromReference(gridReference);
    public GridData GetGridCoordinates(double x, double y) => GetGridCoordinates((int)x, (int)y);
    public GridData GetGridCoordinates(int x, int y) {
        var reference = GetGridReference(x, y);
        return GetGridCoordinatesFromReference(reference);
    }

    public GridData GetGridCoordinatesFromReference(string gridReference) {
        if (string.IsNullOrEmpty(gridReference)) return GridData.Error("Empty or Null Grid Reference.");
        var coordinates = ConvertGridReferenceToCoordinates(gridReference);

        // Calculate the center coordinates of the specified cell
        var topLeftX = HorizontalMargin + (coordinates.col * BoxSize);
        var topLeftY = VerticalMargin + (coordinates.row * BoxSize);
        var centerX  = topLeftX + (BoxSize / 2);
        var centerY  = topLeftY + (BoxSize / 2);
        return new GridData(topLeftX, topLeftY, centerX, centerY, BoxSize); 
    }

    /// <summary>
    ///1 Looks at an X,Y location and works out which cells these would fall into 
    /// to determine the Coordinates as a Xnn format (ie: B12) 
    /// </summary>
    public string GetGridReference((int x, int y) loc) => GetGridReference(loc.x, loc.y);
    public string GetGridReference(int x, int y) {

        if (PanelWidth == 0 || PanelHeight == 0) return string.Empty;
        
        // Calculate the column and row indices
        // Example: Width is 105, Box is 10, x position = 45
        var colIndex = (x-HorizontalMargin) / BoxSize; 
        var rowIndex = (y-VerticalMargin)   / BoxSize;

        // Ensure indices are within bounds
        if (colIndex <= 0) colIndex = 0;
        if (colIndex >= PanelCols) colIndex = PanelCols - 1;
        if (rowIndex <= 0) rowIndex = 0;
        if (rowIndex >= PanelRows) rowIndex = PanelRows - 1;
        
        return ConvertCoordinatesToGridReference(colIndex, rowIndex);
    }
    
    /// <summary>
    /// This function will take a Column and Row and will convert it to a storable
    /// grid reference string. 
    /// </summary>
    private string ConvertCoordinatesToGridReference(int col, int row) {
        if (row < 0 || row > PanelRows || col < 0 || col > PanelCols) return string.Empty;
        var colLetter = (char)('A' + col);        // Convert column index to letter
        var rowNumber = (row + 1).ToString("D2"); // Convert row index to 01-18 format
        return $"{colLetter}{rowNumber}";
    }
    
    /// <summary>
    /// This function will take a Grid Reference and will work out the coordinates of the
    /// grid reference as a Col,Row (X,Y) coordinate.  
    /// </summary>
    private (int col, int row) ConvertGridReferenceToCoordinates(string gridReference) {
        ArgumentException.ThrowIfNullOrEmpty(gridReference, "Grid reference cannot be null or empty");

        // Extract the column letter and row number
        var colLetter = gridReference[0];
        var rowNumberString = gridReference[1..];

        // Convert the column letter to an index (A -> 0, B -> 1, ..., X -> 23)
        var col = colLetter - 'A';
        if (col < 0 || col > PanelCols) {
            throw new ArgumentOutOfRangeException(nameof(gridReference), $"Column must be between A and {'A'+PanelCols-1}");
        }

        // Convert the row number to an index (01 -> 0, 02 -> 1, ..., 18 -> 17)
        if (!int.TryParse(rowNumberString, out var rowNumber)) {
            throw new ArgumentException("Invalid row number", nameof(gridReference));
        }

        var row = rowNumber - 1;
        if (row < 0 || row > PanelRows) {
            throw new ArgumentOutOfRangeException(nameof(gridReference), $"Row must be between 01 and {PanelRows}");
        }
        return (col, row);
    }
}