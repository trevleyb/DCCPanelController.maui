using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

[DebuggerDisplay("{EntityName} is {Type} @ {Col},{Row}")]
[method: JsonConstructor]
public abstract partial class Entity() : ObservableObject, IEntity {
    
    [ObservableProperty] private bool _isEnabled = true; // Is this item actually in use?
    [ObservableProperty] private int _col;               // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _row;               // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _width = 1;         // What width is this component?
    [ObservableProperty] private int _height = 1;        // What Height is this component? 
    [ObservableProperty] private int _rotation;          // Is the track rotated?

    [JsonIgnore] protected abstract int RotationFactor { get; }
    [JsonIgnore] public Panel? Parent { get; set; }
    [JsonIgnore] public Guid Guid { get; init; } = Guid.NewGuid();

    [ObservableProperty] [property: EditableOpacity("Opacity", "Allows for a level of transparency",9,"Visibility")]
    private double _opacity = 1.0;

    [ObservableProperty] [property: EditableInt("Layer", "A higher number places this tile on top of tiles with a lower number.", 9, "Visibility")]
    private int _layer = 1;
    
    protected Entity(Panel panel) : this() {
        Parent = panel;
        Layer = EntityPresets.DefaultLayer(this);  
    }

    protected Entity(Entity entity, params string[] excludeProperties) : this() {
        ArgumentNullException.ThrowIfNull(entity);

        // Always exclude these base properties by default
        var defaultExclusions = new[] { "Parent", "Id", "Guid" };
        var allExclusions = defaultExclusions.Concat(excludeProperties).ToArray();
        ObjectCloner.CloneProperties(entity, this, allExclusions);

        Parent = entity.Parent;
        Guid = Guid.NewGuid();
    }
    public abstract Entity Clone();

    public abstract string EntityName { get; }
    public virtual string Type => GetType().Name;

    public void RotateLeft() {
        Rotation = (Rotation - RotationFactor + 360) % 360;
    }

    public void RotateRight() {
        Rotation = (Rotation + RotationFactor) % 360;
    }

}