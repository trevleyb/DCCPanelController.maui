namespace DCCClients.Jmri.JMRI;

public class JmriSettings : IDccSettings {
    public string JmriServerUrl { get; set; } = "http://localhost:8080";
    public string Name { get; set; } = "JMRI";
    public string Type => "jmri";
}