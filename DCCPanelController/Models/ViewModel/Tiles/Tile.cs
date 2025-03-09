using System.Runtime.CompilerServices;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

using System.ComponentModel;

public abstract partial class Tile : ContentView, ITile
{
    public Entity Entity { get; init; }
    protected Tile(Entity entity, double gridSize) {
        Entity = entity;
        GridSize = gridSize;
        PropertyChanged += OnPropertyChanged;
        entity.PropertyChanged += EntityOnPropertyChanged;
    }

    protected void SetContent(Microsoft.Maui.Controls.View content) {
        Content = content;
        Content.SetBinding(HeightRequestProperty, new Binding(nameof(TileHeight), source: this));
        Content.SetBinding(WidthRequestProperty, new Binding(nameof(TileWidth), source: this));
        Content.SetBinding(ZIndexProperty, new Binding(nameof(Entity.Layer), source: Entity));
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"Tile Property Changed: {e.PropertyName}");
        switch (e.PropertyName) {
            case nameof(GridSize):
                OnPropertyChanged(nameof(TileWidth)) ;
                OnPropertyChanged(nameof(TileHeight)) ;
                break;
            default:
                break;
        }
    }

    private void EntityOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"Entity Property Changed: {e.PropertyName}");
        switch (e.PropertyName) {
            case nameof(Entity.Width): 
                OnPropertyChanged(nameof(TileWidth)) ;
                break;
            case nameof(Entity.Height): 
                OnPropertyChanged(nameof(TileHeight)) ;
                break;
            default:
                break;
        }
    }

    public abstract void CreateTile();
    public bool IsSelected { get; set => SetField(ref field, value); }
    public double GridSize { get; set=> SetField(ref field, value); }
    public double TileWidth => GridSize * Entity.Width;
    public double TileHeight => GridSize * Entity.Height;
    
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "") {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        Console.WriteLine($"{propertyName} changed" );
        
        OnPropertyChanging(propertyName);
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}