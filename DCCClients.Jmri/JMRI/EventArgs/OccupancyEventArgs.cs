using System.Data;
using DCCClients.Jmri.JMRI.DataBlocks;

namespace DCCClients.Jmri.JMRI.EventArgs;

public class OccupancyEventArgs : System.EventArgs {
    public string Identifier { get; set; }
    public string State { get; set; }
    public string TrainId { get; set; }
    public bool IsOccupied { get; set; }
    public string? Metadata { get; set; }
    
    public OccupancyEventArgs(string trainId, string identifier, bool isOccupied, string? metadata) {
        Identifier = identifier;
        IsOccupied = isOccupied;
        State = IsOccupied ? "OCCUPIED" : "FREE";
        TrainId = trainId;
        Metadata = metadata;
    }

    public OccupancyEventArgs(string jsonString) {
        try {
            var occupancyData = OccupancyParser.ParseBlockData(jsonString);
            if (occupancyData is null) throw new DataException("Invalid JSON object for Occupancy block: " + jsonString);
            TrainId = occupancyData.Data.Name ?? "unknown";
            Identifier = occupancyData.Data.UserName ?? "Unknown";
            IsOccupied = occupancyData.Data.State == 2;
            State = IsOccupied ? "OCCUPIED" : "FREE";
        } catch (Exception ex) {
            TrainId = "unknown";
            Identifier = "Unknown";
            IsOccupied = false;
            State = IsOccupied ? "OCCUPIED" : "FREE";
            Console.WriteLine($"Could not parse Occupancy Data: {ex.Message}");
        }
    }
}