namespace DCCClients.Jmri.JMRI;

public class JmriSettings : DccSettings, IDccSettings {
    public override string Name { get; set; } = "JMRI";
    public override string Type => "jmri";
}