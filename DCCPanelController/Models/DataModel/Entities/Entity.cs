using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

[DebuggerDisplay("{EntityName} is {Type} @ {Col},{Row}")]
[method: JsonConstructor]
public abstract partial class Entity() : ObservableObject {
    protected int RotationFactor = 45;
    
    [ObservableProperty] private bool _isEnabled = true; // Is this item actually in use?
    [ObservableProperty] private int _col;               // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _row;               // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _width = 1;         // What width is this component?
    [ObservableProperty] private int _height = 1;        // What Height is this component? 
    [ObservableProperty] private int _rotation;          // Is the track rotated?

    [JsonIgnore] public Panel? Parent { get; set; }
    [JsonIgnore] public Guid Guid { get; init; } = Guid.NewGuid();

    [ObservableProperty] [property: EditableInt("Layer", "", 9, "Track")]
    private int _layer = 1;
    
    protected Entity(Panel panel) : this() {
        Parent = panel;
        Layer = EntityPresets.DefaultLayer(this); // Setup the default layer for this item but can be changed later. 
    }

    protected Entity(Entity entity) : this() {
        Col = entity.Col;
        Row = entity.Row;
        Width = entity.Width;
        Height = entity.Height;
        Rotation = entity.Rotation;
        Layer = entity.Layer;
        IsEnabled = entity.IsEnabled;
        Parent = entity.Parent;
        Guid = Guid.NewGuid();
    }

    public abstract string EntityName { get; }
    public virtual string Type => GetType().Name;

    public void RotateLeft() {
        Rotation = (Rotation - RotationFactor + 360) % 360;
    }

    public void RotateRight() {
        Rotation = (Rotation + RotationFactor) % 360;
    }

    public abstract Entity Clone();
}