using System.Data;
using DCCClients.Jmri.JMRI.DataBlocks;
using DCCClients.Jmri.JMRI.Helpers;

namespace DCCClients.Jmri.JMRI.EventArgs;

public class SignalEventArgs : System.EventArgs {
    public string Identifier { get; set; }
    public string State { get; set; }
    public string? Metadata { get; set; }
    
    public SignalEventArgs(string identifier, string state, string? metadata) {
        Identifier = identifier;
        State = state;
        Metadata = metadata;
    }
    
    public SignalEventArgs(string jsonString) {
        var signalData = SignalMastParser.ParseSignalMastData(jsonString);
        if (signalData is null) throw new DataException("Invalid JSON object for SignalMast: " + jsonString);
        Identifier = signalData.Data.UserName;
        State = signalData.Data.State;
    }

}

