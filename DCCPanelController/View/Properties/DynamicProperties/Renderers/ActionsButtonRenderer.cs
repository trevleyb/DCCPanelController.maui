using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Actions;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class ActionsButtonRenderer : BaseRenderer, IPropertyRenderer {
    protected override int LabelWidth => -1;
    protected override int FieldHeight => -1;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.ButtonActions;

    public object CreateView(PropertyContext ctx) {

        // We need to get the root entity so we can get the available buttons and turnouts
        // -------------------------------------------------------------------------------
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Object: ButtonActions").CreateView(ctx);
        
        try {
            if (entity is IEntityID actionEntity) {
                var entityID = actionEntity.Id ?? "";
                var availableButtons = EntityHelper.GetAllEntitiesByType<ActionButtonEntity>(entity.Parent).Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Id != entityID).Select(b => b.Id).ToList<string>() ?? [];
                
                // Next, get the current value of the Actions' Collection to pass to the control
                // ----------------------------------------------------------------------------
                if (entity is IActionEntity actionContext) {
                    if (ctx.Row.CurrentValue is ButtonActions actionsEntity) {
                        var grid = new ButtonActionsGrid(actionsEntity, actionContext.Context, availableButtons, () => { ctx.Row.IsTouched = true; }) {
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Fill,
                        };
                        return WrapWithLabel(ctx, grid);
                    }
                }
            }
        } catch (Exception ex) {
            return new InvalidRenderer($"Unable to create a Action {ex.Message}");
        }
        return new InvalidRenderer("Entity is not an Action.");
    }
}