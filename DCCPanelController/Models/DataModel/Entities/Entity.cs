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

    public virtual void RotateLeft() {
        Rotation = (Rotation - RotationFactor + 360) % 360;
    }

    public virtual void RotateRight() {
        Rotation = (Rotation + RotationFactor) % 360;
    }
    
    protected void HandleRotation() {
        // Calculate current center point before swapping
        var centerX = Col + (Width - 1) / 2.0;
        var centerY = Row + (Height - 1) / 2.0;
    
        // ALWAYS swap dimensions on every 90-degree rotation
        (Width, Height) = (Height, Width);
    
        // Calculate new position to maintain center
        var newCol = (int)Math.Round(centerX - (Width - 1) / 2.0);
        var newRow = (int)Math.Round(centerY - (Height - 1) / 2.0);
    
        // Apply bounds checking if panel constraints exist
        if (Parent is {} panel) {
            newCol = Math.Max(0, Math.Min(newCol, panel.Cols - Width));
            newRow = Math.Max(0, Math.Min(newRow, panel.Rows - Height));
        }
    
        Col = newCol;
        Row = newRow;
    }

}