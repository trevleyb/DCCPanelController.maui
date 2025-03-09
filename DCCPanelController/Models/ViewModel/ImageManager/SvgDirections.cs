namespace DCCPanelController.Models.ViewModel.ImageManager;

public static class SvgDirections {
    public static SvgDirection GetDirection(int degrees) => (SvgDirection)degrees;
    public static int GetDirection(string direction) => int.Parse(direction);
    public static int GetDirectionIndex(SvgDirection direction) => GetDirectionIndex((int)direction);
    public static int GetDirectionIndex(int degress) => degress / 45;
}

public enum SvgDirection {
    North = 0,
    NorthEast = 45,
    East = 90,
    SouthEast = 135,
    South = 180,
    SouthWest = 225,
    West = 270,
    NorthWest = 315
}