using System.Text.Json;

namespace DCCPanelController.Clients.Jmri.Events;

public interface IJmriProcessor<T> where T : class, IJmriEventArgs {
    static abstract T? Create(JsonElement root);
    static abstract bool HasChanged(T? existingItem, T newItem);
}