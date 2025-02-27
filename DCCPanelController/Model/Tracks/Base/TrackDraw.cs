using DCCPanelController.View.PropertyPages.Attributes;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackDraw(Panel? parent) : Track(parent) {
    [property: EditableInt(Name = "Layer", Group = "Attributes", Description = "What Layer does this peice sit on?", MinValue = 1, MaxValue = 5, Order = 5)]
    public new int Layer {
        get => base.Layer;
        set => base.Layer = value;
    }

}

public enum DrawPosition {
    Left,
    Center,
    Right,
    Top,
    Middle,
    Bottom,
}