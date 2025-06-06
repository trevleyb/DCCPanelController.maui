using System.Data;
using DccClients.Jmri.DataBlocks;

namespace DccClients.Jmri.EventArgs;

public class SignalEventArgs : System.EventArgs {
    public SignalEventArgs(string identifier, string state, string? metadata) {
        Identifier = identifier;
        State = state;
        Metadata = metadata;
    }

    public SignalEventArgs(string jsonString) {
        var signalData = SignalMastParser.ParseSignalMastData(jsonString);
        if (signalData is null) throw new DataException("Invalid JSON object for SignalMast: " + jsonString);
        Identifier = signalData.Data.Name;
        State = signalData.Data.State;
    }

    public string Identifier { get; set; }
    public string State { get; set; }
    public string? Metadata { get; set; }
}