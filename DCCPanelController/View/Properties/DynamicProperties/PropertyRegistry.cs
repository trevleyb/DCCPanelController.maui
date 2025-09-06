namespace DCCPanelController.View.Properties.DynamicProperties;

// UI-agnostic renderer contracts. Concrete MAUI renderers live in the UI project
public interface IPropertyRenderer {
    bool CanRender(PropertyContext ctx);
    object CreateView(PropertyContext ctx); // In MAUI: returns a View/DataTemplate; here it's an object
}

public interface IPropertyRendererRegistry {
    void Register(string editorKind, IPropertyRenderer renderer);
    IPropertyRenderer Resolve(string editorKind);
}

public sealed class PropertyRendererRegistry : IPropertyRendererRegistry {
    private readonly Dictionary<string, IPropertyRenderer> _byKind = new(StringComparer.OrdinalIgnoreCase);
    private readonly IPropertyRenderer _fallback;

    public PropertyRendererRegistry(IPropertyRenderer? fallback = null) {
        _fallback = fallback ?? new NotSupportedRenderer();
    }

    public void Register(string editorKind, IPropertyRenderer renderer) => _byKind[editorKind] = renderer;
    public IPropertyRenderer Resolve(string editorKind) => _byKind.TryGetValue(editorKind, out var r) ? r : _fallback;

    private sealed class NotSupportedRenderer : IPropertyRenderer {
        public int? DefaultWidth => -1;
        public bool CanRender(PropertyContext ctx) => true;
        public object CreateView(PropertyContext ctx) => throw new NotSupportedException($"No renderer for '{ctx.EditorKind}'");
    }
}