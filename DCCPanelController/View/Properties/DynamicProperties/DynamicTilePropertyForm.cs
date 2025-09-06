using System.Globalization;

namespace DCCPanelController.View.Properties.DynamicProperties;

public enum AppMode { Edit, Run }

public sealed class DynamicTilePropertyForm {
    private readonly IEditableExtractor _extractor;
    private readonly IPropertyRendererRegistry _renderers;
    private readonly IValidator _validator;
    private readonly IEqualityPolicy _equality;
    private readonly IUndoService _undo;
    private readonly IEditorKindResolver _kindResolver;

    public AppMode Mode { get; }
    public IReadOnlyList<object> SelectedEntities { get; }
    public IReadOnlyList<PropertyRow> Rows { get; }
    public IReadOnlyList<PropertyGroup> Groups { get; }
    public bool CanApply { get; private set; }

    public static DynamicTilePropertyForm CreateForm(IEnumerable<object> selection) {
        var extractor = new EditableExtractorCache();
        var renderers = new PropertyRendererRegistry();
        PropertyRenderers.RegisterDefaults(renderers);

        var validator = new CompositeValidator([
            new PropertyRendererRules.RequiredRule(),
            new PropertyRendererRules.RangeRule(),
            new PropertyRendererRules.RegexRule()
        ]);

        var equality = new DefaultEqualityPolicy();
        var undo = new DefaultUndoService();
        var kindResolver = new EditableExtractorResolver();

        return new DynamicTilePropertyForm(selection, extractor, renderers, validator, equality, undo, kindResolver, AppMode.Edit);
    }
    
    private DynamicTilePropertyForm(IEnumerable<object> selectedEntities,
                       IEditableExtractor extractor,
                       IPropertyRendererRegistry renderers,
                       IValidator validator,
                       IEqualityPolicy equality,
                       IUndoService undo,
                       IEditorKindResolver kindResolver,
                       AppMode mode = AppMode.Edit) {
        
        SelectedEntities = selectedEntities.ToList();
        _extractor = extractor;
        _renderers = renderers;
        _validator = validator;
        _equality = equality;
        _undo = undo;
        _kindResolver = kindResolver;
        Mode = mode;
        (Groups,Rows) = BuildGroups();
    }

    private (IReadOnlyList<PropertyGroup>,IReadOnlyList<PropertyRow>) BuildGroups() {
        if (SelectedEntities.Count == 0) return ([],[]);

        var first = SelectedEntities[0];
        var fields = _extractor.Extract(first.GetType());

        // Group by attribute Group, then order groups and fields consistently
        var groups = new Dictionary<string, PropertyGroup>(StringComparer.OrdinalIgnoreCase);
        var rows = new List<PropertyRow>(fields.Count);
        
        foreach (var f in fields.OrderBy(f => f.Meta.Group)
                                .ThenBy(f => f.Meta.Order)
                                .ThenBy(f => f.Prop.Name)) {
            
            var gname = string.IsNullOrWhiteSpace(f.Meta.Group) ? "General" : f.Meta.Group;
            if (!groups.TryGetValue(gname, out var g)) {
                g = new PropertyGroup(gname, f.Meta.Order);
                groups[gname] = g;
            }

            var row = new PropertyRow(f);
            var values = SelectedEntities.Select(e => f.Accessor.Get(e)).ToList();
            var firstVal = values[0];
            var allEqual = values.All(v => _equality.AreEqual(v, firstVal, f.Accessor.PropertyType));

            row.OriginalValue = allEqual ? firstVal : null;
            row.CurrentValue = row.OriginalValue;
            row.HasMixedValues = !allEqual;

            g.Rows.Add(row);
            rows.Add(row);
        }
        return (groups.Values
                      .OrderBy(g => g.Name) // primary by name
                      .ThenBy(g => g.Order) // tie-break by first field order
                      .ToList(), rows);
    }

    public object GetRendererView(PropertyRow row) {
        var ctx = new PropertyContext(row, Mode);
        var kind = _kindResolver.Resolve(row.Field);
        ctx.EditorKind = kind;
        var renderer = _renderers.Resolve(kind);
        return !renderer.CanRender(ctx) ? throw new NotSupportedException($"Renderer cannot render kind '{kind}'") : renderer.CreateView(ctx);
    }

    public async Task<ValidationSummary> ValidateAsync() {
        var summary = await _validator.ValidateAsync(this, Rows).ConfigureAwait(false);
        CanApply = !summary.HasErrors;
        return summary;
    }

    public IReadOnlyList<PropertyChange> PreviewDiff() {
        var changes = new List<PropertyChange>();
        foreach (var row in Rows) {
            var shouldApply = row.IsTouched || !_equality.AreEqual(row.CurrentValue, row.OriginalValue, row.Field.Accessor.PropertyType);
            if (!shouldApply) continue;
            foreach (var entity in SelectedEntities) {
                var oldVal = row.Field.Accessor.Get(entity);
                var newVal = row.CurrentValue;
                if (_equality.AreEqual(oldVal, newVal, row.Field.Accessor.PropertyType)) continue;
                changes.Add(new PropertyChange(entity, row.Field, oldVal, newVal));
            }
        }
        return changes;
    }

    public async Task<bool> ApplyAsync(bool requireAtomic = false) {
        var summary = await ValidateAsync().ConfigureAwait(false);
        if (summary.HasErrors) {
            Console.WriteLine("Validation failed.");
            return false;
        }

        var changes = PreviewDiff();
        if (changes.Count == 0) {
            Console.WriteLine("No changes to apply.");
            return true;
        }

        var tx = new ApplyTransaction(changes);
        try {
            await tx.CommitAsync().ConfigureAwait(false);
            _undo.Push(tx);
            foreach (var row in Rows) {
                row.OriginalValue = row.CurrentValue;
                row.IsTouched = false;
                var values = SelectedEntities.Select(e => row.Field.Accessor.Get(e)).ToList();
                var firstVal = values[0];
                row.HasMixedValues = !values.All(v => _equality.AreEqual(v, firstVal, row.Field.Accessor.PropertyType));
            }
            return true;
        } catch when (!requireAtomic) {
            return false;
        } catch {
            await tx.RollbackAsync().ConfigureAwait(false);
            return false;
        }
    }
}