using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public abstract partial class TrackTile(Entity entity, double gridSize) : Tile(entity, gridSize) {
    protected SvgStyleBuilder SetDefaultStyles() {
        var style = new SvgStyleBuilder();

        if (Entity is TrackEntity trackEntity) {

            switch (trackEntity.TrackType) {
            case TrackTypeEnum.MainLine:
                style.Add(e => e.WithName(SvgElementType.Border).WithColor(trackEntity.TrackBorderColor ?? Entity.Parent?.BorderColor ?? Colors.Black).Visible())
                     .Add(e => e.WithName(SvgElementType.Track).WithColor(trackEntity.TrackColor ?? Entity.Parent?.MainLineColor ?? Colors.Black).Visible())
                     .Add(e => e.WithName(SvgElementType.Occupied).Hidden());
                break;

            case TrackTypeEnum.BranchLine:
                style.Add(e => e.WithName(SvgElementType.Border).Hidden())
                     .Add(e => e.WithName(SvgElementType.Track).WithColor(trackEntity.TrackColor ?? Entity.Parent?.BranchLineColor ?? Colors.Gray).Visible())
                     .Add(e => e.WithName(SvgElementType.Occupied).Hidden());
                break;
            }

            switch (trackEntity.TrackAttribute) {
            case TrackAttributeEnum.Hidden:
                style.Add(e => e.WithName(SvgElementType.Dashline).WithColor(Entity.Parent?.HiddenColor ?? Colors.White).Visible());
                break;

            case TrackAttributeEnum.Normal:
                style.Add(e => e.WithName(SvgElementType.Dashline).Hidden());
                break;
            }
        }
        return style;
    } 
}