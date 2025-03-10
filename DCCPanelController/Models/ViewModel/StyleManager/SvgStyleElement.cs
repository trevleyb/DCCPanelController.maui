namespace DCCPanelController.Models.ViewModel.StyleManager;

public class SvgStyleElement {
    internal SvgStyleElement(string name) {
        Name = name;
    }

    public string Name { get; internal set; }
    public Dictionary<string, string> Attributes { get; private set; } = new();

    public static SvgElementBuilder Builder(string name) {
        return new SvgElementBuilder(name);
    }

    public void AddOrUpdateAttribute(string key, string value) {
        Attributes.TryAdd(key, value);
        Attributes[key] = value;
    }

    public override string ToString() {
        return $"Element: {Name}, EditableAttribute: [{string.Join(", ", Attributes.Select(kv => $"{kv.Key}={kv.Value}"))}]";
    }
}
