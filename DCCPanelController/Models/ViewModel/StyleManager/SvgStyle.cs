namespace DCCPanelController.Models.ViewModel.StyleManager;

public class SvgStyle {
    public Dictionary<string, SvgStyleElement> Elements { get; } = [];

    public static SvgStyleBuilder Builder() {
        return new SvgStyleBuilder();
    }

    public void AddElement(SvgStyleElement element) {
        Elements.TryAdd(element.Name, element);
        foreach (var attr in element.Attributes) {
            Elements[element.Name].AddOrUpdateAttribute(attr.Key, attr.Value);
        }
    }

    public void MergeStyle(SvgStyle style) {
        foreach (var element in style.Elements.Values) {
            AddElement(element);
        }
    }
}