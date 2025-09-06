using System.Globalization;

namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed class FormContext {
    private readonly IEditableExtractor _extractor;
    private readonly IPropertyRendererRegistry _renderers;
    private readonly IValidator _validator;
    private readonly IEqualityPolicy _equality;
    private readonly IUndoService _undo;
    private readonly IEditorKindResolver _kindResolver;

    public AppMode Mode { get; }
    public IReadOnlyList<object> SelectedEntities { get; }
    public IReadOnlyList<PropertyRow> Rows { get; }
    public bool CanApply { get; private set; }

    public FormContext(IEnumerable<object> selectedEntities,
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
        Rows = BuildRows();
    }

    private IReadOnlyList<PropertyRow> BuildRows() {
        if (SelectedEntities.Count == 0) return [];
        var first = SelectedEntities[0];
        var fields = _extractor.Extract(first.GetType());
        var rows = new List<PropertyRow>(fields.Count);

        foreach (var f in fields) {
            var row = new PropertyRow(f);
            var values = SelectedEntities.Select(e => f.Accessor.Get(e)).ToList();
            var firstVal = values[0];
            var allEqual = values.All(v => _equality.AreEqual(v, firstVal, f.Accessor.PropertyType));
            row.OriginalValue = allEqual ? firstVal : null;
            row.CurrentValue = row.OriginalValue;
            row.HasMixedValues = !allEqual;
            rows.Add(row);
        }
        return rows;
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
            bool shouldApply = row.IsTouched || !_equality.AreEqual(row.CurrentValue, row.OriginalValue, row.Field.Accessor.PropertyType);
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
        if (summary.HasErrors) return false;
        
        var changes = PreviewDiff();
        if (changes.Count == 0) return true;
        
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
