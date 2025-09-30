using System.Text.Json;
using DCCPanelController.Clients.Jmri.Helpers;

namespace DCCPanelController.Clients.Jmri.Events;

public class JmriSensorEventArgs : JmriEventArgs<JmriSensorEventArgs>, IJmriEventArgs, IJmriProcessor<JmriSensorEventArgs> {
    private JmriSensorEventArgs() { }
    public string UserName { get; private set; } = string.Empty;
    public int State { get; private set; } // 2=Active, 4=Inactive
    public bool Inverted { get; private set; }
    public string Name { get; private init; } = string.Empty;

    public static JmriSensorEventArgs? Create(JsonElement root) {
        if (!root.TryGetProperty("data", out var dataElement)) return null;
        return new JmriSensorEventArgs {
            Name = dataElement.GetStringProperty("name"),
            UserName = dataElement.GetStringProperty("userName"),
            State = dataElement.GetIntProperty("state"),
            Inverted = dataElement.GetBoolProperty("inverted")
        };
    }

    public static bool HasChanged(JmriSensorEventArgs? existingItem, JmriSensorEventArgs newItem) {
        return existingItem == null ||
               existingItem?.State != newItem.State;
    }
}