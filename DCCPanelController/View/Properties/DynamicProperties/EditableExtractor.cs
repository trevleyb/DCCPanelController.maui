using System.Reflection;

namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed record EditableField(
    Type DeclaringType,
    PropertyInfo Prop,
    EditableAttribute Meta,
    IPropertyAccessor Accessor);

public interface IPropertyAccessor {
    Type PropertyType { get; }
    string Name { get; }
    object? Get(object target);
    void Set(object target, object? value);
}

public interface IEditableExtractor {
    IReadOnlyList<EditableField> Extract(Type type);
}

public sealed class CompiledPropertyAccessor : IPropertyAccessor {
    private readonly Func<object, object?>   _getter;
    private readonly Action<object, object?> _setter;

    public CompiledPropertyAccessor(string name, Type propertyType, Func<object, object?> getter, Action<object, object?> setter) {
        Name = name;
        PropertyType = propertyType;
        _getter = getter;
        _setter = setter;
    }

    public Type PropertyType { get; }
    public string Name { get; }

    public object? Get(object target) => _getter(target);
    public void Set(object target, object? value) => _setter(target, value);
}

public sealed class EditableExtractorCache : IEditableExtractor {
    private readonly Dictionary<Type, IReadOnlyList<EditableField>> _cache = new();

    public IReadOnlyList<EditableField> Extract(Type type) {
        if (_cache.TryGetValue(type, out var cached)) return cached;
        var list = new List<EditableField>();

        foreach (var pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
            var meta = pi.GetCustomAttribute<EditableAttribute>(true);
            if (meta == null || !pi.CanRead || !pi.CanWrite) continue;
            var accessor = CompileAccessor(pi);
            list.Add(new EditableField(type, pi, meta, accessor));
        }

        var ordered = list.OrderBy(f => f.Meta.Group).ThenBy(f => f.Meta.Order).ThenBy(f => f.Prop.Name).ToList();
        _cache[type] = ordered;
        return ordered;
    }

    private static IPropertyAccessor CompileAccessor(PropertyInfo pi) {
        object? Getter(object target) => pi.GetValue(target);
        void Setter(object target, object? value) => pi.SetValue(target, value);
        return new CompiledPropertyAccessor(pi.Name, pi.PropertyType, Getter, Setter);
    }
}