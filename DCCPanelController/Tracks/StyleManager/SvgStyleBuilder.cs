using System.Diagnostics;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.StyleManager;

[Serializable]
[DebuggerDisplay("{Name}")]
public class SvgStyle {
    public string Name { get; internal set; }
    public Dictionary<string, SvgStyleElement> Elements { get; private set; } = [];

    internal SvgStyle(string name) {
        Name = name;
    }

    public static SvgStyleBuilder Builder(string name) {
        return new SvgStyleBuilder(name);
    }

    public void AddElement(SvgStyleElement element) {
        if (Elements.ContainsKey(element.Name)) {
            foreach (var attr in element.Attributes) {
                Elements[element.Name].AddOrUpdateAttribute(attr.Key, attr.Value);
            }
        } else {
            Elements[element.Name] = element;
        }
    }

    public void MergeStyle(SvgStyle style) {
        foreach (var element in style.Elements.Values) {
            AddElement(element);
        }
    }

    public override string ToString() {
        return $"Style: {Name}, Elements: [{string.Join(", ", Elements.Keys)}]";
    }
}

[Serializable]
[DebuggerDisplay("{Name}")]
public class SvgStyleElement {
    public string Name { get; internal set; }
    public Dictionary<string, string> Attributes { get; private set; } = new();

    internal SvgStyleElement(string name) {
        Name = name;
    }

    public static SvgElementBuilder Builder(string name) {
        return new SvgElementBuilder(name);
    }

    public void AddOrUpdateAttribute(string key, string value) {
        Attributes[key] = value;
    }

    public override string ToString() {
        return $"Element: {Name}, Attributes: [{string.Join(", ", Attributes.Select(kv => $"{kv.Key}={kv.Value}"))}]";
    }
}

public class SvgStyleBuilder(string name) {
    private readonly SvgStyle _style = new(name);

    public SvgStyleBuilder AddElement(Action<SvgElementBuilder> buildElement) {
        var builder = new SvgElementBuilder(string.Empty);
        buildElement(builder);
        var element = builder.Build();
        _style.AddElement(element);
        return this;
    }

    public SvgStyleBuilder AddExistingStyle(SvgStyle existingStyle) {
        _style.MergeStyle(existingStyle);
        return this;
    }

    public SvgStyleBuilder AddExistingStyle(string existingStyleName) {
        _style.MergeStyle(SvgStyles.GetStyle(existingStyleName));
        return this;
    }

    public SvgStyle Build() {
        return _style;
    }
}

public class SvgElementBuilder(string name) {
    private readonly SvgStyleElement _element = new(name);

    public SvgElementBuilder WithName(SvgElementEnum name) {
        _element.Name = SvgElement.ToString(name);
        return this;
    }

    public SvgElementBuilder WithName(string name) {
        _element.Name = name;
        return this;
    }

    public SvgElementBuilder AddAttribute(string key, string value) {
        _element.AddOrUpdateAttribute(key, value);
        return this;
    }

    public SvgElementBuilder WithOpacity(int opacity) {
        return WithOpacity(opacity.ToString());
    }

    public SvgElementBuilder WithOpacity(string opacity) {
        _element.AddOrUpdateAttribute("Opacity", opacity);
        return this;
    }

    public SvgElementBuilder WithColor(Color color) {
        return WithColor(color.ToArgbHex());
    }

    public SvgElementBuilder WithColor(string color) {
        _element.AddOrUpdateAttribute("Color", color);
        return this;
    }

    public SvgElementBuilder Dashed(bool isDashed = true) {
        _element.AddOrUpdateAttribute("Dashed", isDashed.ToString());
        return this;
    }

    public SvgElementBuilder StandardTrack() {
        _element.AddOrUpdateAttribute("Dashed", false.ToString());
        return this;
    }

    public SvgElementBuilder HiddenTrack() {
        _element.AddOrUpdateAttribute("Dashed", true.ToString());
        return this;
    }

    public SvgElementBuilder Hidden() {
        return Visible(false);
    }

    public SvgElementBuilder Visible(bool isVisible = true) {
        _element.AddOrUpdateAttribute("Visible", isVisible.ToString());
        return this;
    }

    public SvgStyleElement Build() {
        return _element;
    }
}