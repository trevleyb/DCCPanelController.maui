using System.Text.Json;
using DccClients.Jmri.Events;
using DccClients.Jmri.Helpers;

namespace DccClients.Jmri.Events;

public class JmriSensorEventArgs : System.EventArgs {
    public string Name { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public int State { get; private set; } // 2=Active, 4=Inactive
    public bool Inverted { get; private set; }

    private JmriSensorEventArgs() { }

    public static JmriSensorEventArgs? Create(JsonElement root) {
        if (!root.TryGetProperty("data", out var dataElement)) return null;
        return new JmriSensorEventArgs() {
            Name = dataElement.GetStringProperty("name"),
            UserName = dataElement.GetStringProperty("userName"),
            State = dataElement.GetIntProperty("state"),
            Inverted = dataElement.GetBoolProperty("inverted")
        };
    }   

}