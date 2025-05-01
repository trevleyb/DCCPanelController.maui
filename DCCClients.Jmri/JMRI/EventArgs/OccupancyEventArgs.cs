using System.Data;
using DCCClients.Jmri.JMRI.DataBlocks;

namespace DCCClients.Jmri.JMRI.EventArgs;

public class OccupancyEventArgs : System.EventArgs {
    public string Identifier { get; set; }
    public bool IsOccupied { get; set; }
    public string? State { get; set; }
    public string? TrainId { get; set; }
    public string? Metadata { get; set; }
    
    public OccupancyEventArgs(string identifier, bool isOccupied, string?  state , string? trainId, string? metadata) {
        Identifier = identifier;
        IsOccupied = isOccupied;
        State = state;
        TrainId = trainId;
        Metadata = metadata;
    }

    public OccupancyEventArgs(string jsonString) {
        var occupancyData = OccupancyParser.ParseBlockData(jsonString);
        if (occupancyData is null) throw new DataException("Invalid JSON object for Occupancy block: " + jsonString);
        Identifier = occupancyData.Data.UserName;
        IsOccupied = occupancyData.Data.State == 2;
        State = occupancyData.Data.State == 2 ? "OCCUPIED" : "FREE";
        TrainId = occupancyData.Data.Name;
    }
}