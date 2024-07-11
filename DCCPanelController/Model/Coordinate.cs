namespace DCCPanelController.Model;

public class Coordinate {

    private int _col;
    private int _row;

    public bool IsValid { get; set; } = true;
    
    public Coordinate(int col, int row, bool? isValid = true) {
        _col = col;
        _row = row;
        IsValid = isValid ?? true;
        //Reference = GetReference(col, row);
    }

    //public string Reference { get; set; }

    public int Column {
        get => _col; //GetCoordinates(Reference).col;
        set {
            _col = value;
            //Reference = GetReference(_col, _row);
        }
    }
    
    public int Row {
        get => _row; //GetCoordinates(Reference).row;
        set {
            _row = value;
            //Reference = GetReference(_col, _row);
        }
    }

    public static Coordinate Unreferenced => new Coordinate(-1, -1, false);
    
    //public void SetCoordinates(int col, int row) => Reference = GetReference(col, row);
    
    public override string ToString() => $"{Column},{Row}";
    
    /// <summary>
    /// This function will take a Column and Row and will convert it to a storable
    /// grid reference string. 
    /// </summary>
    //private string GetReference(int col, int row) {
    //    if (col == -1 || row == -1) return "*00";
    //    var colLetter = (char)('A' + col);        // Convert column index to letter
    //    var rowNumber = (row + 1).ToString("D2"); // Convert row index to 01-18 format
    //    return $"{colLetter}{rowNumber}";
    //}
    
    /// <summary>
    /// This function will take a Grid Reference and will work out the coordinates of the
    /// grid reference as a Col,Row (X,Y) coordinate.  
    /// </summary>
    //private (int col, int row) GetCoordinates(string gridReference) {
    //     if (string.IsNullOrEmpty(gridReference)) return (-1, -1);
    //     if (gridReference.Equals("*00")) return (-1, -1);
    //
    //     // Extract the column letter and row number
    //     var colLetter = gridReference[0];
    //     var rowNumberString = gridReference[1..];
    //
    //     // Convert the column letter to an index (A -> 0, B -> 1, ..., X -> 23)
    //     var col = colLetter - 'A';
    //     if (col < 0) col = 0;
    //     if (col > 26) col = 25;
    //
    //     // Convert the row number to an index (01 -> 0, 02 -> 1, ..., 18 -> 17)
    //     if (!int.TryParse(rowNumberString, out var rowNumber)) {
    //         rowNumber = 1;
    //     }
    //     var row = rowNumber - 1;
    //     if (row < 0) row = 0;
    //     if (row > 26) row = 25;
    //     return (col, row);
    // }
}