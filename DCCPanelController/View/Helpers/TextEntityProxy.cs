using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

public class TextEntityProxy : ObservableObject, IDisposable {
    private ITextEntity? _entity;

    public ITextEntity? Entity {
        get => _entity;
        set {
            if (_entity == value)
                return;

            if (_entity is INotifyPropertyChanged oldNpc)
                oldNpc.PropertyChanged -= OnEntityPropertyChanged;

            _entity = value;
            OnPropertyChanged();

            if (_entity is INotifyPropertyChanged newNpc)
                newNpc.PropertyChanged += OnEntityPropertyChanged;

            OnPropertyChanged(nameof(EditText)); // ensure UI refreshes
        }
    }

    public string? EditText {
        get => Entity?.Label;
        set {
            if (Entity is null) return;
            if (Entity.Label == value) return;
            Entity.Label = value ?? string.Empty;
            OnPropertyChanged(nameof(EditText));
        }
    }

    private void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ITextEntity.Label))
            OnPropertyChanged(nameof(EditText));
    }

    public void Dispose() {
        if (_entity is INotifyPropertyChanged npc)
            npc.PropertyChanged -= OnEntityPropertyChanged;
    }
}