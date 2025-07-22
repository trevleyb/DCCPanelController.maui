using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCCPanelController.Services;

public class AppState : INotifyPropertyChanged {
    // Singleton pattern to ensure there's only one instance
    private static readonly AppState _instance = new();
    public static AppState Instance => _instance;

    private bool _isEditingPanel;

    public bool IsEditingPanel {
        get => _isEditingPanel;
        set {
            if (_isEditingPanel == value) return;
            _isEditingPanel = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNavigationAllowed));
        }
    }

    /// <summary>
    /// A convenience property that is the inverse of IsEditingPanel.
    /// This makes binding in XAML cleaner.
    /// </summary>
    public bool IsNavigationAllowed => !IsEditingPanel;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}