using System.Xml.Linq;

namespace DCCPanelController.Components.Tracks.SVGManager;

public class SvgAttribute (XDocument svgImage) : SvgModifier (document: svgImage), ISvgModifier {

    public void SetElementOccupied(Color? color, int? opacity = null) {}
    public void SetElementFree(Color? color, int? opacity = null) {}
    public void SetElementRoute(Color? color, int? opacity = null) {}
    
    public void SetButtonColor(Color? color, int? opacity = null)       => SetElementColor(ButtonElementName, color, opacity);
    public void SetTrackColor(Color? color, int? opacity = null)        => SetElementColor(TrackElementName, color, opacity);
    public void SetBorderColor(Color? color, int? opacity = null)       => SetElementColor(BorderElementName, color, opacity);
    public void SetDivergingColor(Color? color, int? opacity = null)    => SetElementColor(DivergingElementName, color, opacity);
    public void SetTerminatorColor(Color? color, int? opacity = null)   => SetElementColor(TerminatorElementName, color, opacity);
    public void SetContinuationColor(Color? color, int? opacity = null) => SetElementColor(ContinuationElementName, color, opacity);

    public void SetElementColor(string elementName, Color? color, int? opacity) {
        foreach (var element in FindElements(elementName)) {
            if (color is not null) SvgModifier.SetAttributeValue(element, FillAttributeName, color.ToHex());
            if (opacity is >= 0 and <= 100) SvgModifier.SetAttributeValue(element, FillOpacityName, opacity.ToString() ?? "100");
        }
    }
}