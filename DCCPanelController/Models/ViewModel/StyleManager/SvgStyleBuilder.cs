namespace DCCPanelController.Models.ViewModel.StyleManager;

public class SvgStyleBuilder {
    private readonly SvgStyle _style = new();

    public SvgStyle Build() {
        return _style;
    }

    public SvgStyleBuilder Add(Action<SvgElementBuilder> buildElement) {
        var builder = new SvgElementBuilder(string.Empty);
        buildElement(builder);
        var element = builder.Build();
        _style.AddElement(element);
        return this;
    }

    public SvgStyleBuilder AddExisting(SvgStyleBuilder existingStyle) {
        _style.MergeStyle(existingStyle.Build());
        return this;
    }

    public SvgStyleBuilder AddExisting(SvgStyle existingStyle) {
        _style.MergeStyle(existingStyle);
        return this;
    }
}

public class SvgElementBuilder(string name) {
    private readonly SvgStyleElement _element = new(name);

    public SvgElementBuilder WithName(SvgElementType svgElement) {
        _element.Name = SvgElementTypes.GetElement(svgElement);
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
        return WithColor(color?.ToArgbHex() ?? Colors.Black.ToArgbHex());
    }

    public SvgElementBuilder WithColor(string color) {
        _element.AddOrUpdateAttribute("Color", color);
        return this;
    }

    public SvgElementBuilder Color(Color color) {
        return WithColor(color.ToArgbHex());
    }

    public SvgElementBuilder Color(string color) {
        _element.AddOrUpdateAttribute("Color", color);
        return this;
    }

    public SvgElementBuilder Text(string text) {
        _element.AddOrUpdateAttribute("Text", text);
        return this;
    }

    public SvgElementBuilder WithTextSize(int textSize) {
        _element.AddOrUpdateAttribute("Font-Size", textSize.ToString());
        return this;
    }

    public SvgElementBuilder WithTextSize(string textSize) {
        _element.AddOrUpdateAttribute("Font-Size", textSize);
        return this;
    }

    public SvgElementBuilder WithTextRegular() {
        _element.AddOrUpdateAttribute("Font-Weight", FontWeight.Regular.ToString());
        return this;
    }

    public SvgElementBuilder WithTextThin() {
        _element.AddOrUpdateAttribute("Font-Weight", FontWeight.Thin.ToString());
        return this;
    }

    public SvgElementBuilder WithTextBold() {
        _element.AddOrUpdateAttribute("Font-Weight", FontWeight.Bold.ToString());
        return this;
    }

    public SvgElementBuilder WithTextWeight(FontWeight weight) {
        _element.AddOrUpdateAttribute("Font-Weight", weight.ToString());
        return this;
    }

    public SvgElementBuilder WithTextColor(string color) {
        _element.AddOrUpdateAttribute("Font-Color", color);
        return this;
    }

    public SvgElementBuilder WithTextColor(Color color) {
        _element.AddOrUpdateAttribute("Font-Color", color.ToArgbHex());
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