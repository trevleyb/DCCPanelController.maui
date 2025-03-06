using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.DataModel.Tracks;

namespace DCCPanelController.Model.DataModel;

public partial class Actions<T> : ObservableCollection<Action<T>> where T : Enum { }

public partial class Action<T> : ObservableObject where T : Enum {
        [ObservableProperty] private string _id = string.Empty;
        [ObservableProperty] private T _whenOnOrClosed = default(T);
        [ObservableProperty] private T _whenOffOrThrown = default(T);
        [ObservableProperty] private bool _cascade;
}
