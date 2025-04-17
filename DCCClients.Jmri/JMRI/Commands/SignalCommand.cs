using DCCClients.Common;

namespace DCCClients.Jmri.JMRI.Commands;

public class SignalCommand {
    public string Identifier { get; set; }
    public SignalAspectEnum Aspect { get; set; }

    public SignalCommand(string identifier, SignalAspectEnum aspect) {
        Identifier = identifier;
        Aspect = aspect;
    }

    /// <summary>
    /// Converts the signal aspect to a string representation that JMRI understands
    /// </summary>
    public string GetAspectString() {
        return Aspect switch {
            SignalAspectEnum.Red => "RED",
            SignalAspectEnum.Yellow => "YELLOW",
            SignalAspectEnum.Green => "GREEN",
            SignalAspectEnum.FlashRed => "FLASHRED",
            SignalAspectEnum.FlashYellow => "FLASHYELLOW",
            SignalAspectEnum.FlashGreen => "FLASHGREEN",
            SignalAspectEnum.RedYellow => "RED_OVER_YELLOW",
            SignalAspectEnum.RedGreen => "RED_OVER_GREEN",
            SignalAspectEnum.YellowGreen => "YELLOW_OVER_GREEN",
            SignalAspectEnum.AllOn => "DARK",
            _ => "DARK"
        };
    }
}
