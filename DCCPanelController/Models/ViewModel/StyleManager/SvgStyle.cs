using System.Text;

namespace DCCPanelController.Models.ViewModel.StyleManager;

public class SvgStyle {
    public Dictionary<string, SvgStyleElement> Elements { get; } = [];

    public static SvgStyleBuilder Builder() => new();

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

    public override string ToString() {
        var sb = new StringBuilder();
        foreach (var element in Elements) {
            sb.Append(element.Value.ToString());
        }
        return sb.ToString();
    }
}