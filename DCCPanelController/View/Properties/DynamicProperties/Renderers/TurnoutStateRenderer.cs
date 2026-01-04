using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Components;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class TurnoutStateRenderer : BaseRenderer, IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.TurnoutState;

    public object CreateView(PropertyContext ctx) {
        try {
        var row = ctx.Row;
        var state = row.OriginalValue is TurnoutStateEnum ? (TurnoutStateEnum)row.OriginalValue : TurnoutStateEnum.Unknown;

        var cell = new TurnoutStateControl {
            CanToggleState = true,
            State = state,
        };
        cell.StateChanged += (sender, newState) => SetValue(row, newState);
        cell.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, cell);
        } catch (Exception ex) {
            Logger.LogError(ex, "Error creating Turnout State Renderer for property {PropertyName}", ctx.Row?.Field?.Meta?.Label);
            return new InvalidRenderer(ex.Message).CreateView(ctx);
        }
    }
}