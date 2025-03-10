using System.Runtime.CompilerServices;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

using System.ComponentModel;

public abstract partial class Tile : ContentView, ITile {
    public Entity Entity { get; init; }

    public bool IsSelected { get; set => SetField(ref field, value); }
    public double GridSize { get; set => SetField(ref field, value); }

    public double TileWidth => GridSize * Entity.Width;
    public double TileHeight => GridSize * Entity.Height;

    protected Tile(Entity entity, double gridSize) {
        Entity = entity;
        GridSize = gridSize;
        PropertyChanged += OnPropertyChanged;
        entity.PropertyChanged += EntityOnPropertyChanged;
    }

    public abstract void CreateTile();
    public void RotateLeft() => Rotation = (Rotation - 45 + 360) % 360;
    public void RotateRight() => Rotation = (Rotation + 45) % 360;

    protected void SetContent(Microsoft.Maui.Controls.View content) {
        content.ZIndex = Entity.Layer;
        Content = content;
        Content.SetBinding(HeightRequestProperty, new Binding(nameof(TileHeight), source: this));
        Content.SetBinding(WidthRequestProperty, new Binding(nameof(TileWidth), source: this));
        Content.SetBinding(ZIndexProperty, new Binding(nameof(Entity.Layer), source: Entity));
        Content.SetBinding(IsVisibleProperty, new Binding(nameof(IsEnabled), source: Entity));
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"Tile Property Changed: {e.PropertyName}");
        switch (e.PropertyName) {
        case nameof(GridSize):
            OnPropertyChanged(nameof(TileWidth));
            OnPropertyChanged(nameof(TileHeight));
            break;
        case nameof(ImageSource) or nameof(Content):
            break;
        default:
            // If any properties of the Tile such as states or sizes have changed,
            // then we need to rebuild the tile. 
            // -------------------------------------------------------------------------
            CreateTile();
            break;
        }
    }

    private void EntityOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"Entity Property Changed: {e.PropertyName}");
        CreateTile();

        // switch (e.PropertyName) {
        //     case nameof(Entity.Width): 
        //         OnPropertyChanged(nameof(TileWidth)) ;
        //         break;
        //     case nameof(Entity.Height): 
        //         OnPropertyChanged(nameof(TileHeight)) ;
        //         break;
        //     default:
        //         break;
        // }
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "") {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        Console.WriteLine($"{propertyName} changed");

        OnPropertyChanging(propertyName);
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}