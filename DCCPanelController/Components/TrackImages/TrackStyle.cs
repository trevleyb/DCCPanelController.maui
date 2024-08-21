namespace DCCPanelController.Components.TrackImages;

public class TrackStyle(string name) {
    public string Name { get; set; } = name;
    public List<StyleElement> StyleElements { get; } = new List<StyleElement>();

    public void AddElement(StyleElement element) {
        StyleElements.Add(element);
    }
    
    public void ApplyStyle(TrackImage trackImage) {
        trackImage.ApplyStyle(this);
    }
}

public class StyleElement(string name) {
    public string Name { get; set; } = name;
    public List<StyleAttribute> Attributes { get; } = [];

    public void AddAttribute(StyleAttribute attribute) {
        Attributes.Add(attribute);
    }
}

public class StyleAttribute {
    public string Name { get; set; }
    public string Value { get; set; }

    public StyleAttribute(string name, string value) {
        Name = name;
        Value = value;
    }
}

