using DCCClients.Interfaces;

namespace DCCClients.JMRI;

public class JmriSettings : IDccSettings {    
    public string Name { get; set; } = "JMRI";
    public string Type => "jmri";
    public string JmriServerUrl { get; set; } = "http://localhost:8080";
}