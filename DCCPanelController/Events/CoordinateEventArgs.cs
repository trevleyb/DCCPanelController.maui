namespace DCCPanelController.Events;

public class CoordinatesEventArgs(int x, int y) : EventArgs {
    public int Col { get; } = x;
    public int Row { get; } = y;
}