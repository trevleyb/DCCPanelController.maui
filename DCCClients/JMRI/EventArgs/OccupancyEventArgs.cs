namespace DCCClients.JMRI.EventArgs;

public class OccupancyEventArgs : System.EventArgs {
    /// <summary>
    ///     The unique identifier of the block or section.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the block is occupied.
    /// </summary>
    public bool IsOccupied { get; set; }

    /// <summary>
    ///     The train identifier, if any, occupying the block.
    /// </summary>
    public string? TrainId { get; set; }

    /// <summary>
    ///     Additional data, such as speed or direction, if available.
    /// </summary>
    public string? Metadata { get; set; }
}