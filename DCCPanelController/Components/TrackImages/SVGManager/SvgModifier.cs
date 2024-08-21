using System.Xml.Linq;

namespace DCCPanelController.Components.Tracks.SVGManager;

public class SvgModifier (XDocument document) {
    
    protected const string FillAttributeName       = "fill";
    protected const string FillOpacityName         = "fill-opacity";
    protected const string BorderElementName       = "Border";
    protected const string TrackElementName        = "Track";
    protected const string ButtonElementName       = "Button";
    protected const string ContinuationElementName = "Continuation";
    protected const string DivergingElementName    = "Diverging";
    protected const string TerminatorElementName   = "Track";

    
    #region Manage Changing Colors and Opacity using the attribute for the tem to change
    protected List<XElement> FindElements(string elementName) => FindElementsAttribute("", "id", elementName).ToList();
    protected List<XElement> FindElementsAttribute(string elementName, string attributeName, string attributeValue) {
        ArgumentNullException.ThrowIfNull(document);
        var elements = new List<XElement>();
        foreach (var element in document.Descendants()) {
            if (string.IsNullOrEmpty(elementName) || element.Name.LocalName.Equals(elementName, StringComparison.OrdinalIgnoreCase)) {
                foreach (var attr in element.Attributes()) {
                    if (attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) && attr.Value.Equals(attributeValue, StringComparison.OrdinalIgnoreCase)) {
                        elements.Add(element);
                    }
                }
            }
        }
        return elements;
    }
    
    protected static void SetAttributeValue(XElement element, string attributeName, string attributeValue) {
        ArgumentNullException.ThrowIfNull(element);
        var attribute = (from attr in element.Attributes() where attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) select attr).FirstOrDefault();
        if (attribute is not null) {
            attribute.Value = attributeValue;
        } else {
            element.Add(new XAttribute(attributeName, attributeValue));
        }
    }

    protected static string? GetAttributeValue(XElement element, string attributeName) {
        ArgumentNullException.ThrowIfNull(element);
        return (from attr in element.Attributes() where attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) select attr.Value).FirstOrDefault();
    }
    #endregion
}