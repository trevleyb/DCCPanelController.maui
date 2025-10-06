using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class UniqueIDRenderer : BaseRenderer, IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Text;

    public object CreateView(PropertyContext ctx) {
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Object: UniqueID Renderer").CreateView(ctx);

        var row = ctx.Row;
        var entry = new Entry { Text = row.OriginalValue as string ?? string.Empty, Placeholder = MixedPlaceholder(row), TextColor = Colors.Black, };
        entry.TextChanged += (s, e) => {
            if (IsIDValid(e.NewTextValue, entity)) {
                SetValue(row, e.NewTextValue);
                entry.TextColor = FieldColor;
            } else {
                entry.TextColor = ErrorColor;
            }
        };
        entry.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, AddBorder(entry));
    }

    private bool IsIDValid(string value, IEntity entity) {
        var isValid = true;
        if (entity is IEntityGeneratingID entityIDs) {
            var ids = EntityHelper.GetAllEntitiesByType<IEntityID>(entity.Parent);
            var conflictingEntities = ids?.Where(x => x.Id == value).ToArray() ?? [];
            isValid = conflictingEntities.Length is 0 or 1;
        }
        return isValid;
    }
}