using DCCCommon;

namespace DCCClients.Jmri.JMRI;

public class JmriSettings : DccSettings, IDccSettings {
    public new string Name { get; set; } = "JMRI";
    public new string Type => "jmri";
}