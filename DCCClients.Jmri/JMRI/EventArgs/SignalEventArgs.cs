using System.Data;
using DCCClients.Common;
using DCCClients.Jmri.JMRI.DataBlocks;
using DCCClients.Jmri.JMRI.Helpers;

namespace DCCClients.Jmri.JMRI.EventArgs;

public class SignalEventArgs : System.EventArgs {
    /// <summary>
    ///     The unique identifier of the signal.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    ///     The current aspect of the signal.
    /// </summary>
    public SignalAspectEnum Aspect { get; set; } = SignalAspectEnum.Off;

    /// <summary>
    ///     The raw state string from JMRI, if available.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    ///     Additional metadata or information about the signal.
    /// </summary>
    public string? Metadata { get; set; }
    
    public SignalEventArgs(string identifier, SignalAspectEnum aspect, string state, string? metadata) {
        Identifier = identifier;
        Aspect = aspect;
        State = state;
        Metadata = metadata;
    }
    
    public SignalEventArgs(string jsonString) {
        var signalData = SignalMastParser.ParseSignalMastData(jsonString);
        if (signalData is null) throw new DataException("Invalid JSON object for SignalMast: " + jsonString);
        Identifier = signalData.Data.UserName;
        Aspect =  ConvertStateToAspect(signalData.Data.Aspect);
        State = signalData.Data.State;
    }
    
    private SignalAspectEnum ConvertStateToAspect(string state) {
        return state.ToUpperInvariant() switch {
            "RED"               => SignalAspectEnum.Red,
            "YELLOW"            => SignalAspectEnum.Yellow,
            "GREEN"             => SignalAspectEnum.Green,
            "FLASHRED"          => SignalAspectEnum.FlashRed,
            "FLASHYELLOW"       => SignalAspectEnum.FlashYellow,
            "FLASHGREEN"        => SignalAspectEnum.FlashGreen,
            "RED_OVER_YELLOW"   => SignalAspectEnum.RedYellow,
            "RED_OVER_GREEN"    => SignalAspectEnum.RedGreen,
            "YELLOW_OVER_GREEN" => SignalAspectEnum.YellowGreen,
            "DARK"              => SignalAspectEnum.Off,
            _                   => SignalAspectEnum.Off
        };
    }

}

