using System.Text.Json.Serialization;

namespace DCCPanelController.Components.TrackImages;

[Serializable]
public class TrackStyle(string name, List<StyleElement>? elements = null) {
    public string Name { get; set; } = name;
    public List<StyleElement> Elements { get; set; } = elements ?? [];
    public void AddElement(StyleElement element) => Elements.Add(element);
    public void ApplyStyle(TrackImage trackImage) => trackImage.ApplyStyle(this);
}

[Serializable]
public class StyleElement(string name, List<StyleAttribute>? attributes = null) {
    public string Name { get; set; } = name;
    public List<StyleAttribute> Attributes { get; set; } = attributes ?? [];
    public void AddAttribute(StyleAttribute attribute) => Attributes.Add(attribute);
}

[Serializable]
public class StyleAttribute(string name, string value) {
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;
}
