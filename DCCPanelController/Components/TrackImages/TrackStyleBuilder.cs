namespace DCCPanelController.Components.TrackImages;

public class TrackStyleBuilder(string styleName) {
    public string Name => _style.Name;
    private readonly TrackStyle _style = new(styleName);

    public StyleElementBuilder AddElement(string elementType) {
        var element = new StyleElement(elementType);
        _style.AddElement(element);
        return new StyleElementBuilder(this, element);
    }

    public TrackStyle Build() {
        return _style;
    }
}

public class StyleElementBuilder(TrackStyleBuilder styleBuilder, StyleElement element) {
    public StyleElementBuilder AddAttribute(string attributeName, string attributeValue) {
        var attribute = new StyleAttribute(attributeName, attributeValue);
        element.AddAttribute(attribute);
        return this;
    }
    
    public StyleElementBuilder Opacity(string opacity) {
        var attribute = new StyleAttribute("Opacity", opacity);
        element.AddAttribute(attribute);
        return this;
    }

    public StyleElementBuilder Color(string attributeValue) {
        var attribute = new StyleAttribute("Color", attributeValue);
        element.AddAttribute(attribute);
        return this;
    }
    
    public StyleElementBuilder Visible(bool isVisible = true) {
        var attribute = new StyleAttribute("Opacity", isVisible ? "100" : "0");
        element.AddAttribute(attribute);
        return this;
    }
    
    public StyleElementBuilder Hidden(bool isVisible = false) => Visible(isVisible); 
    
    public StyleElementBuilder Dashed(bool isDashed = true) {
        var attribute = new StyleAttribute("Dashed", isDashed ? "1" : "0");
        element.AddAttribute(attribute);
        return this;
    }
    
    public TrackStyleBuilder Done() {
        return styleBuilder;
    }
}