using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class ButtonStateRenderer : BaseRenderer,IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.ButtonState;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var state = row.OriginalValue is ButtonStateEnum ? (ButtonStateEnum)row.OriginalValue : ButtonStateEnum.Unknown; 
            
        var cell = new ButtonStateControl() {
            CanToggleState = true,
            State = state,
        };
        cell.StateChanged += (sender, newState) => SetValue(row, newState); 
        cell.IsEnabled = !(row.Field.Meta.IsReadOnlyInRunMode);        
        return WrapWithLabel(ctx, cell);
    }
}