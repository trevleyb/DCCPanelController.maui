using System.Data;
using DCCClients.Jmri.JMRI.DataBlocks;

namespace DCCClients.Jmri.JMRI.EventArgs;

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
    
    public OccupancyEventArgs(string identifier, bool isOccupied, string? trainId, string? metadata) {
        Identifier = identifier;
        IsOccupied = isOccupied;
        TrainId = trainId;
        Metadata = metadata;
    }

    public OccupancyEventArgs(string jsonString) {
        var occupancyData = OccupancyParser.ParseBlockData(jsonString);
        if (occupancyData is null) throw new DataException("Invalid JSON object for Occupancy block: " + jsonString);
        Identifier = occupancyData.Data.UserName;
        IsOccupied = occupancyData.Data.State != 0;
        TrainId = occupancyData.Data.Name;
    }
}