using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

[DebuggerDisplay("{Name}:{TrackClass}: {Col},{Row}")]
[method: JsonConstructor]
public abstract partial class Entity() : ObservableObject {

    public abstract string Name { get; }
    public virtual string Type => GetType().Name;
    
    [ObservableProperty] private int _col;                 // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _row;                 // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _layer = 1;           // What position (layer) should this exist at 
    [ObservableProperty] private int _width  = 1;          // What width is this component?
    [ObservableProperty] private int _height = 1;          // What Height is this component? 
    [ObservableProperty] private int _rotation = 0;        // Is the track rotated?
    [ObservableProperty] private bool _isEnabled = true;   // Is this item actually in use?
    
    [ObservableProperty]
    [property:JsonIgnore]
    private bool _isSelected = false; // Is this item actually in use?
    
    [JsonIgnore] public Panel? Parent { get; set; } = null;
    [JsonIgnore] public Guid Guid { get; init; } = Guid.NewGuid();

    protected Entity(Panel panel) : this() { 
        Parent = panel;
        Layer = EntityPresets.DefaultLayer(this);   // Setup the default layer for this item but can be changed later. 
    }
    protected Entity(Entity entity) : this() {
        Col = entity.Col;
        Row = entity.Row;
        Layer = entity.Layer;
        Width = entity.Width;
        Height = entity.Height;
        Rotation = entity.Rotation;
        IsEnabled = entity.IsEnabled;
        IsSelected = false;
    }
    public abstract Entity Clone();
}