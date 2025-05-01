using System.Data;
using System.Text.Json;
using DCCClients.Jmri.JMRI.DataBlocks;
using DCCClients.Jmri.JMRI.Helpers;

namespace DCCClients.Jmri.JMRI.EventArgs;

public class TurnoutEventArgs : System.EventArgs {
    public string Identifier { get; set; }
    public int? DccAddress { get; set; }
    public string State { get; set; }
    public bool Inverted { get; set; }

    public TurnoutEventArgs(string identifier, int? dccAddress, string state) {
        Identifier = identifier;
        DccAddress = dccAddress;
        State = state;
        Inverted = false;
    }

    public TurnoutEventArgs(string jsonString) {
        var turnoutData = TurnoutParser.ParseTurnoutData(jsonString);
        if (turnoutData is null) throw new DataException("Invalid JSON object for Turnout: " + jsonString);
        Identifier = turnoutData.Data.UserName;
        DccAddress = turnoutData.Data.Name.ConvertToDCCAddress();
        State = turnoutData.Data.State == 4 ? "THROWN" : "CLOSED";
        Inverted = turnoutData.Data.Inverted;
    }
}