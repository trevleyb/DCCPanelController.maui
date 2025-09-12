using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Actions;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class ActionsTurnoutRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldHeight => -1;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.TurnoutActions;
    public object CreateView(PropertyContext ctx) {
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Object: TurnoutActions").CreateView(ctx);
        try {
            if (entity is IEntityID actionEntity) {
                var entityID = actionEntity.Id ?? "";
                var availableTurnouts = actionEntity.AllIDs().Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Id != entityID).Select(b => b.Id).ToList<string>() ?? [];
                if (entity is IActionEntity actionsEntity) {
                    var grid = new TurnoutActionsGrid(actionsEntity, actionsEntity.Context, availableTurnouts) {
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill
                    };
                    return WrapWithLabel(ctx, grid);
                }
            }
        } catch (Exception ex) {
            return new InvalidRenderer($"Unable to create a Action {ex.Message}");
        }
        return new InvalidRenderer($"Entity is not an Action.");
    }
}