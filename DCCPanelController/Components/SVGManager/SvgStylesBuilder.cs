namespace DCCPanelController.Components.TrackImages;

public class SvgStylesBuilder(string styleName) {
    public string Name => _style.Name;
    private readonly SvgStyle _style = new(styleName);

    public StyleElementBuilder AddElement(string elementType) {
        var element = new StyleElement(elementType);
        _style.AddElement(element);
        return new StyleElementBuilder(this, element);
    }

    public SvgStyle Build() {
        return _style;
    }
}

public class StyleElementBuilder(SvgStylesBuilder stylesBuilder, StyleElement element) {
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

    public StyleElementBuilder Color(Color attributeValue) {
        var attribute = new StyleAttribute("Color", attributeValue.ToArgbHex());
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
    
    public SvgStylesBuilder Done() {
        return stylesBuilder;
    }
}