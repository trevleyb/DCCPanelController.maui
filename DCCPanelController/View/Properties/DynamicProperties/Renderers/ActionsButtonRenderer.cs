using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Actions;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class ActionsButtonRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldHeight => -1;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.ButtonActions;
    public object CreateView(PropertyContext ctx) {
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Object: ButtonActions").CreateView(ctx);
        try {
            // TODO: This was changed previously to use the ID of the entity, but that doesn't work.
            var entityID = (entity as IEntityID)?.Id ?? "";
            var availableButtons = entity?.Parent?.GetAllEntitiesByType<ActionButtonEntity>().Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Id != entityID).Select(b => b.Id).ToList<string>() ?? [];
            if (entity is IActionEntity actionsEntity) {
                var grid = new ButtonActionsGrid(actionsEntity, ctx.Row.Field.Meta.ActionsContext, availableButtons) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
                return WrapWithLabel(ctx, grid);
            }
        } catch (Exception ex) {
            return new InvalidRenderer($"Unable to create a Action {ex.Message}");
        }
        return new InvalidRenderer($"Entity is not an Action.");
    }
}