using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Properties.DynamicProperties.Renderers;

namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed class DynamicTilePropertyForm {
    private const double SmallScreenWidth = 500;

    private static readonly List<string> GroupOrders = [
        "General",
        "Text", "Track", "Circle", "Rectangle",
        "Turnout", "Layout", "Attributes", "Colors", "Color", "Actions", "Action", "Button Actions", "Turnout Actions", "Dimensions", "Visibility",
    ];

    private readonly IEqualityPolicy                                     _equality;
    private readonly IEditableExtractor                                  _extractor;
    private readonly double                                              _height;
    private readonly IEditorKindResolver                                 _kindResolver;
    private readonly IPropertyRendererRegistry                           _renderers;
    private readonly IUndoService                                        _undo;
    private readonly IValidator                                          _validator;
    private readonly double                                              _width;
    private          Dictionary<Type, Dictionary<string, EditableField>> _fieldsByTypeName = new();

    /// <summary>
    ///     This is the main form that we create to show in a Popup Page
    /// </summary>
    private DynamicTilePropertyForm(IEnumerable<Entity> selectedEntities,
        IEditableExtractor extractor,
        IPropertyRendererRegistry renderers,
        IValidator validator,
        IEqualityPolicy equality,
        IUndoService undo,
        IEditorKindResolver kindResolver,
        double width, double height) {
        SelectedEntities = selectedEntities.ToList();
        _extractor = extractor;
        _renderers = renderers;
        _validator = validator;
        _equality = equality;
        _undo = undo;
        _width = width;
        _height = height;
        _kindResolver = kindResolver;
        (Groups, Rows) = BuildGroups();
        HasCommonProperties = Rows.Count > 0;
        CanApply = false;
    }

    public IReadOnlyList<Entity> SelectedEntities { get; }
    public IReadOnlyList<PropertyRow> Rows { get; }
    public IReadOnlyList<PropertyGroup> Groups { get; }

    public bool CanApply { get; private set; }
    public bool HasCommonProperties { get; }

    public static DynamicTilePropertyForm CreateForm(IEnumerable<Entity> selection,
        double width, double height) {

        var extractor = new EditableExtractorCache();
        var renderers = new PropertyRendererRegistry();
        EditorKinds.RegisterDefaults(renderers);
        var validator = new CompositeValidator([
            new PropertyRendererRules.RequiredRule(),
            new PropertyRendererRules.RangeRule(),
            new PropertyRendererRules.RegexRule(),
        ]);

        var equality = new DefaultEqualityPolicy();
        var undo = new DefaultUndoService();
        var kindResolver = new EditableExtractorResolver();
        return new DynamicTilePropertyForm(selection, extractor, renderers, validator, equality, undo, kindResolver, width, height);
    }

    private static Type UnwrapNullable(Type t) => Nullable.GetUnderlyingType(t) ?? t;

    private (IReadOnlyList<PropertyGroup>, IReadOnlyList<PropertyRow>) BuildGroups() {
        if (SelectedEntities.Count == 0) return([], []);

        // 1) Index fields for every entity type
        // ---------------------------------------------------------------------------------------
        _fieldsByTypeName = new Dictionary<Type, Dictionary<string, EditableField>>();
        foreach (var t in SelectedEntities.Select(e => e.GetType()).Distinct()) {
            var nameMap = _extractor.Extract(t).ToDictionary(f => f.Prop.Name, f => f, StringComparer.Ordinal);
            _fieldsByTypeName[t] = nameMap;
        }

        // 2) Compute intersection of property names across all types
        // ---------------------------------------------------------------------------------------
        var commonNames = _fieldsByTypeName.Values
                                           .Select(d => d.Keys)
                                           .Aggregate((IEnumerable<string>?)null,
                                                (acc, keys) => acc == null ? keys : acc.Intersect(keys, StringComparer.Ordinal))
                                          ?.ToList() ?? new List<string>();

        // 3) Keep only names whose CLR types match across all types
        // ---------------------------------------------------------------------------------------
        commonNames = commonNames
                     .Where(name => {
                          var distinctTypes = _fieldsByTypeName.Values
                                                               .Select(d => UnwrapNullable(d[name].Accessor.PropertyType))
                                                               .Distinct()
                                                               .ToList();
                          var distinctEditorKinds = _fieldsByTypeName.Values
                                                                     .Select(d => _kindResolver.Resolve(d[name], _width))
                                                                     .Distinct()
                                                                     .ToList();
                          return distinctTypes.Count == 1 && distinctEditorKinds.Count == 1;
                      })
                     .ToList();

        if (commonNames.Count == 0) return([], []);

        // 4) Build groups + rows off a representative field (from the first entity’s type)
        // ---------------------------------------------------------------------------------------
        var firstType = SelectedEntities[0].GetType();
        var groups = new Dictionary<string, PropertyGroup>(StringComparer.OrdinalIgnoreCase);
        var flatRows = new List<PropertyRow>();

        // order by Group, Order, then Name — using the representative field’s metadata
        // ---------------------------------------------------------------------------------------
        var orderedNames = commonNames
                          .OrderBy(name => _fieldsByTypeName[firstType][name].Meta.Group)
                          .ThenBy(name => _fieldsByTypeName[firstType][name].Meta.Order)
                          .ThenBy(name => name, StringComparer.Ordinal)
                          .ToList();

        foreach (var name in orderedNames) {
            var repField = _fieldsByTypeName[firstType][name];
            var gname = string.IsNullOrWhiteSpace(repField.Meta.Group) ? "General" : repField.Meta.Group;

            // Create a group but lookup the Group name to get the sort order.
            // --------------------------------------------------------------
            if (!groups.TryGetValue(gname, out var group)) {
                var order = FindGroupIndex(gname);
                group = new PropertyGroup(gname, order);
                groups[gname] = group;
            }

            var row = new PropertyRow(repField);

            // gather values using the correct accessor for each entity's concrete type
            var values = SelectedEntities.Select(e => {
                                              var ef = _fieldsByTypeName[e.GetType()][name];
                                              return ef.Accessor.Get(e);
                                          })
                                         .ToList();

            var firstVal = values[0];
            var allEqual = values.All(v => _equality.AreEqual(v, firstVal, repField.Accessor.PropertyType));

            row.OriginalValue = allEqual ? firstVal : null;
            row.CurrentValue = row.OriginalValue;
            row.HasMixedValues = !allEqual;

            group.Rows.Add(row);
            flatRows.Add(row);
        }

        var orderedGroups = groups.Values
                                  .OrderBy(g => g.Order)
                                  .ThenBy(g => g.Name, StringComparer.Ordinal)
                                  .ToList();

        return(orderedGroups, flatRows);
    }

    public static int FindGroupIndex(string groupName) {
        for (var i = 0; i < GroupOrders.Count; i++) {
            if (groupName.Equals(GroupOrders[i], StringComparison.InvariantCultureIgnoreCase)) return i;
        }
        return 0;
    }

    public object GetRendererView(PropertyRow row) {
        var kind = _kindResolver.Resolve(row.Field, _width);
        var ctx = new PropertyContext(kind, row, _width, _height, SelectedEntities);
        var renderer = _renderers.Resolve(kind);
        return!renderer.CanRender(ctx) ? new InvalidRenderer($"Invalid Renderer: {kind}").CreateView(ctx) : renderer.CreateView(ctx);
    }

    public async Task<ValidationSummary> ValidateAsync() {
        if (!HasCommonProperties) {
            CanApply = false;
            return new ValidationSummary([]);
        }

        var summary = await _validator.ValidateAsync(this, Rows).ConfigureAwait(false);
        CanApply = !summary.HasErrors && Rows.Count > 0;
        return summary;
    }

    private IReadOnlyList<PropertyChange> PreviewDiff() {
        var changes = new List<PropertyChange>();
        foreach (var row in Rows) {
            var shouldApply = row.IsTouched || !_equality.AreEqual(row.CurrentValue, row.OriginalValue, row.Field.Accessor.PropertyType);
            if (!shouldApply) continue;

            var propName = row.Field.Prop.Name;

            foreach (var entity in SelectedEntities) {
                var ef = _fieldsByTypeName[entity.GetType()][propName];
                var oldVal = ef.Accessor.Get(entity);
                var newVal = row.CurrentValue;

                if (_equality.AreEqual(oldVal, newVal, ef.Accessor.PropertyType)) continue;

                // record the concrete field for the entity’s type so Apply/Rollback can use it
                changes.Add(new PropertyChange(entity, ef, oldVal, newVal));
            }
        }
        return changes;
    }

    public async Task<bool> ApplyAsync(bool requireAtomic = false) {
        if (!HasCommonProperties) return false;
        var summary = await ValidateAsync().ConfigureAwait(false);
        if (summary.HasErrors || !CanApply) return false;

        var changes = PreviewDiff();
        if (changes.Count == 0) return true;

        var tx = new ApplyTransaction(changes);
        try {
            await tx.CommitAsync().ConfigureAwait(false);
            _undo.Push(tx);

            foreach (var row in Rows) {
                row.OriginalValue = row.CurrentValue;
                row.IsTouched = false;

                var propName = row.Field.Prop.Name;
                var values = SelectedEntities.Select(e => {
                                                  var ef = _fieldsByTypeName[e.GetType()][propName];
                                                  return ef.Accessor.Get(e);
                                              })
                                             .ToList();

                var firstVal = values[0];
                row.HasMixedValues = !values.All(v => _equality.AreEqual(v, firstVal, row.Field.Accessor.PropertyType));
            }
            return true;
        } catch (Exception ex) when (!requireAtomic) {
            Console.WriteLine("Requires Atomic? rolling back: " + ex.Message);
            return false;
        } catch (Exception ex) {
            Console.WriteLine("Apply failed, rolling back: " + ex.Message);
            await tx.RollbackAsync().ConfigureAwait(false);
            return false;
        }
    }
}