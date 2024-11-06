namespace DCCPanelController.Helpers;

// Helper to get back the Location Data from calculating where a Grid Reference should be
// X and Y offsets are the Top Left corner and X and Y centers are the center point. 
// These take into account any margins within the confines of the view. 
// --------------------------------------------------------------------------------------------------
public class GridData(bool ok, int? xOffset, int? yOffset, int? xCenter, int? yCenter, int? xMargin, int? yMargin, int? boxSize) {
    public int XOffset { get; init; } = xOffset ?? -1;
    public int YOffset { get; init; } = yOffset ?? -1;
    public int XCenter { get; init; } = xCenter ?? -1;
    public int YCenter { get; init; } = yCenter ?? -1;
    public int XMargin { get; init; } = xMargin ?? 0;
    public int YMargin { get; init; } = yMargin ?? 0;
    public int BoxSize { get; init; } = boxSize ?? -1;
    public bool IsOk { get; init; } = ok;
    public string ErrorMessage { get; init; } = string.Empty;

    public GridData(int? xOffset, int? yOffset, int? xCenter, int? yCenter, int? xMargin, int? yMargin, int? boxSize) : this(true, xOffset, yOffset, xCenter, yCenter, xMargin, yMargin, boxSize) { }

    // Helper if the data is invalid. Up to the caller to check the result
    // -------------------------------------------------------------------
    public static GridData Error(string? error = "") {
        return new GridData(false, null, null, null, null, null, null, null) { ErrorMessage = error ?? "" };
    }
}