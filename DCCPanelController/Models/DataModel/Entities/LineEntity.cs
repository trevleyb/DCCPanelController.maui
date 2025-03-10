using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class LineEntity : Entity, IDrawingEntity {
    public override string Name => "Line";
    [ObservableProperty] private Color _lineColor = Colors.Black;
    [ObservableProperty] private int _lineWidth = 3;
    [ObservableProperty] private double _opacity = 1;

    [JsonConstructor]
    public LineEntity() { }
    public LineEntity(Panel panel) : this() {
        Parent = panel;
    }
    public LineEntity(LineEntity entity) : base(entity) {
        LineColor = entity.LineColor;
        LineWidth = entity.LineWidth;
        Opacity = entity.Opacity;
    }
    public override Entity Clone() => new LineEntity(this);
}