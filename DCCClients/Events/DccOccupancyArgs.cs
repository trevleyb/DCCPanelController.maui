namespace DCCClients.Events;

public class DccOccupancyArgs(string dccAddress, string blockId, bool isOccupied) : EventArgs {
    public string DccAddress { get; } = dccAddress;
    public string BlockId { get; } = blockId;
    public bool IsOccupied { get; } = isOccupied;
    public bool IsFree => !IsOccupied;
}