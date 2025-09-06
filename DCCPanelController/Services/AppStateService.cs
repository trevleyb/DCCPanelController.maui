using System.ComponentModel;
using System.Runtime.CompilerServices;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Services;

public class AppStateService : INotifyPropertyChanged {

    public event Action<ITile>? SelectedTileSet;
    public event Action? SelectedTileCleared;

    public static AppStateService Instance { get; } = new();
    
    public ITile? SelectedTile {
        get;
        set {
            if (field == value) return;

            Console.WriteLine($"APPSTATE: Selected Tile Changed: {field?.Entity.EntityName} -> {value?.Entity.EntityName}");
            field = value;
            OnPropertyChanged();
            if (field is not null) {
                SelectedTileSet?.Invoke(field);
            } else {
                SelectedTileCleared?.Invoke();
            }
        }
    }
    
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