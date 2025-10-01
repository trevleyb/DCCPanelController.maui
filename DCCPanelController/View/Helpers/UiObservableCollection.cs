using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DCCPanelController.View.Helpers;

public class UiObservableCollection<T> : ObservableCollection<T>
{
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
        void Raise() => base.OnCollectionChanged(e);
        if (MainThread.IsMainThread) Raise();
        else MainThread.BeginInvokeOnMainThread(Raise);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
        void Raise() => base.OnPropertyChanged(e);
        if (MainThread.IsMainThread) Raise();
        else MainThread.BeginInvokeOnMainThread(Raise);
    }
}
