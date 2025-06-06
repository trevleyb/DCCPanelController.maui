using System.Data;
using DccClients.Jmri.DataBlocks;
using DccClients.Jmri.Helpers;

namespace DccClients.Jmri.EventArgs;

public class TurnoutEventArgs : System.EventArgs {
    public TurnoutEventArgs(string identifier, int? dccAddress, string state) {
        Identifier = identifier;
        DccAddress = dccAddress;
        State = state;
        Inverted = false;
    }

    public TurnoutEventArgs(string jsonString) {
        var turnoutData = TurnoutParser.ParseTurnoutData(jsonString);
        if (turnoutData is null) throw new DataException("Invalid JSON object for Turnout: " + jsonString);
        Identifier = turnoutData.Data.Name;
        DccAddress = turnoutData.Data.Name.ConvertToDCCAddress();
        State = turnoutData.Data.State == 4 ? "THROWN" : "CLOSED";
        Inverted = turnoutData.Data.Inverted;
    }

    public string Identifier { get; set; }
    public int? DccAddress { get; set; }
    public string State { get; set; }
    public bool Inverted { get; set; }
}