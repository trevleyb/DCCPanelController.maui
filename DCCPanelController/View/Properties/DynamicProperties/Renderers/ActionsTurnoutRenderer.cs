using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Accessories;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Actions;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class ActionsTurnoutRenderer : BaseRenderer, IPropertyRenderer {
    protected override int LabelWidth => -1;
    protected override int FieldHeight => -1;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.TurnoutActions;

    public object CreateView(PropertyContext ctx) {
        
        // We need to get the root entity so we can get the available buttons and turnouts
        // -------------------------------------------------------------------------------
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Object: TurnoutActions").CreateView(ctx);
        
        try {
            if (entity is IEntityID actionEntity) {
                var entityID = actionEntity.Id ?? "";
                //var availableTurnouts = EntityHelper.GetAllEntitiesByType<TurnoutEntity>(entity.Parent).Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Id != entityID).Select(b => b.Id).ToList<string>() ?? [];

                var availableTurnouts =
                    (entity.Parent?.Turnouts ?? Enumerable.Empty<Turnout>())
                    .Select(t => t.SystemId).Where(id => id is { }).Cast<string>().ToList();
                
                // Next, get the current value of the Actions' Collection to pass to the control
                // ----------------------------------------------------------------------------
                if (entity is IActionEntity actionContext) {
                    if (ctx.Row.CurrentValue is TurnoutActions actionsEntity) {
                        var grid = new TurnoutActionsGrid(actionsEntity, actionContext.Context, availableTurnouts, () => { ctx.Row.IsTouched = true; }) {
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