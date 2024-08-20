using System.Text;
using System.Xml.Linq;

namespace DCCPanelController.Components.Tracks.SVGManager;

public class SvgStyle (XDocument svgImage) : SvgModifier (svgImage), ISvgModifier {

    public void SetElementOccupied(Color? color, int? opacity = null) {}
    public void SetElementFree(Color? color, int? opacity = null) {}
    public void SetElementRoute(Color? color, int? opacity = null) {}
    
    public void SetButtonColor(Color? color, int? opacity = null)       => SetElementColor(ButtonElementName, color, opacity);
    public void SetTrackColor(Color? color, int? opacity = null)        => SetElementColor(TrackElementName, color, opacity);
    public void SetBorderColor(Color? color, int? opacity = null)       => SetElementColor(BorderElementName, color, opacity);
    public void SetDivergingColor(Color? color, int? opacity = null)    => SetElementColor(DivergingElementName, color, opacity);
    public void SetTerminatorColor(Color? color, int? opacity = null)   => SetElementColor(TerminatorElementName, color, opacity);
    public void SetContinuationColor(Color? color, int? opacity = null) => SetElementColor(ContinuationElementName, color, opacity);


    #region Manage Changing Colors and Opacity using the 'style' attribute
    public void SetElementColor(string elementName, Color? color, int? opacity) {
        var stylesToChange = new Dictionary<string, string>();
        if (color is not null) stylesToChange.Add($"{FillAttributeName}", color.ToHex());
        if (opacity is not null) stylesToChange.Add($"{FillOpacityName}", opacity.ToString() ?? "100");
        foreach (var attribute in stylesToChange) {
            SetElementStyle(elementName, attribute.Key, attribute.Value);
        }
    }

    public void SetElementStyle(string elementName, string styleName, string styleValue) {
        // If we pass "Track" or "Border" this will find all entries in the XDocument where
        // there is an id="" to that value. 
        // -------------------------------------------------------------------------------
        foreach (var element in FindElementsAttribute(elementName, "id", "style")) {
            SetStyleValue(element, styleName, styleValue);
        }
    }

    private void SetStyleValue(XElement element, string styleName, string styleValue) {
        ArgumentNullException.ThrowIfNull(element);
        var styles = GetStyleValues(element);
        styles[styleName] = styleValue;
        SetStyleValues(element, styles);
    }

    private void SetStyleValues(XElement element, Dictionary<string, string> styleValues) {
        var stylesValue = new StringBuilder();
        foreach (var style in styleValues) {
            stylesValue.Append($"{style.Key}:{style.Value}");
        }
        SvgModifier.SetAttributeValue(element, "style", stylesValue.ToString());
    }

    private Dictionary<string, string> GetStyleValues(XElement element) {
        var styleAttributes = new Dictionary<string, string>();
        var stylesAttr = SvgModifier.GetAttributeValue(element, "style");
        if (!string.IsNullOrEmpty(stylesAttr)) {
            var styles = stylesAttr.Split(';');
            foreach (var style in styles) {
                var styleParts = style.Split(':');
                if (styleParts.Length == 2) {
                    styleAttributes.Add(styleParts[0], styleParts[1]);
                }
            }
        }

        return styleAttributes;
    }
    #endregion

    
}