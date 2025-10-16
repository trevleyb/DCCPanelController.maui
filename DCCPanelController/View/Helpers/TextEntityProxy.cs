using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Helpers;

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

            OnPropertyChanged(nameof(EditText)); 
            OnPropertyChanged(nameof(TextSize)); 
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

    public int TextSize {
        get => Entity?.FontSize ?? 14;
        set {
            if (Entity is null) return;
            if (Entity.FontSize == value) return;
            Entity.FontSize = value;
            OnPropertyChanged(nameof(TextSize));
        }
    }

    private void OnEntityPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ITextEntity.Label)) OnPropertyChanged(nameof(EditText));
        if (e.PropertyName == nameof(ITextEntity.FontSize)) OnPropertyChanged(nameof(TextSize));
    }

    public void Dispose() {
        if (_entity is INotifyPropertyChanged npc)
            npc.PropertyChanged -= OnEntityPropertyChanged;
    }
}