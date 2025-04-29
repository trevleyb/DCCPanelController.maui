using System.Data;
using System.Text.Json;
using DCCClients.Jmri.JMRI.DataBlocks;
using DCCClients.Jmri.JMRI.Helpers;

namespace DCCClients.Jmri.JMRI.EventArgs;

public class TurnoutEventArgs : System.EventArgs {
    /// <summary>
    ///     The unique identifier of the turnout.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    ///     The DCC address of the turnout, if applicable.
    /// </summary>
    public int? DccAddress { get; set; }

    /// <summary>
    ///     The current state of the turnout (e.g., "THROWN", "CLOSED").
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the turnout is locked.
    /// </summary>
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
        State = turnoutData.Data.State == 0 ? "THROWN" : "CLOSED";
        Inverted = turnoutData.Data.Inverted;
    }
}