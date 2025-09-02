using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCCPanelController.Services;

public class AppState : INotifyPropertyChanged {

    public static AppState Instance { get; } = new();
    
    public bool IsEditingPanel {
        get;
        set {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNavigationAllowed));
        }
    }

    /// <summary>
    ///     A convenience property that is the inverse of IsEditingPanel.
    ///     This makes binding in XAML cleaner.
    /// </summary>
    public bool IsNavigationAllowed => !IsEditingPanel;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}