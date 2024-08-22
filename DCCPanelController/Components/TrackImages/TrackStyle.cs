using System.Diagnostics;
using System.Text.Json.Serialization;

namespace DCCPanelController.Components.TrackImages;

[Serializable]
[DebuggerDisplay("{Name}")]
public class TrackStyle(string name, List<StyleElement>? elements = null) {
    public string Name { get; set; } = name;
    public List<StyleElement> Elements { get; set; } = elements ?? [];
    public void AddElement(StyleElement element) => Elements.Add(element);
    public void ApplyStyle(TrackImage trackImage) => trackImage.ApplyStyle(this);
}

[Serializable]
[DebuggerDisplay("{Name}")]
public class StyleElement(string name, List<StyleAttribute>? attributes = null) {
    public string Name { get; set; } = name;
    public List<StyleAttribute> Attributes { get; set; } = attributes ?? [];
    public void AddAttribute(StyleAttribute attribute) => Attributes.Add(attribute);
}

[Serializable]
[DebuggerDisplay("{Name}={Value}")]
public class StyleAttribute(string name, string value) {
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;
}
