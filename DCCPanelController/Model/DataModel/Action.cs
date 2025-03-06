using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.DataModel.Tracks;

namespace DCCPanelController.Model.DataModel;

public class Actions<T> : ObservableCollection<Action<T>> where T : Enum {
    public Actions() { }
    public Actions(Actions<T> actions) {
        foreach (var action in actions) Add(new Action<T>(action));
    }
}

public partial class Action<T> : ObservableObject where T : Enum {
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private T _whenOnOrClosed = default(T);
    [ObservableProperty] private T _whenOffOrThrown = default(T);
    [ObservableProperty] private bool _cascade;
    
    [JsonConstructor]
    public Action() { }

    public Action(Action<T> action) {
        Id = action.Id;
        WhenOnOrClosed = action.WhenOnOrClosed;
        WhenOffOrThrown = action.WhenOffOrThrown;
        Cascade = action.Cascade;
    }
}