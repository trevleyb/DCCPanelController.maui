using DCCPanelController.Components.Elements.Base;
using DCCPanelController.Components.Elements.ViewModels;
using DCCPanelController.Components.Elements.Views;
using DCCPanelController.Model;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Components.Elements;

public static class ElementFactory {

    public static IElementView GetElementView(PanelElement element) {
        var elementView = element.Type switch {
            "Straight"     => new TrackView(new StraightElementViewModel()),
            "Terminate"    => new TrackView(new TerminatorElementViewModel()),
            "Crossing"     => new TrackView(new CrossElementViewModel()),
            "Left"         => new TrackView(new LeftElementViewModel()),
            "Right"        => new TrackView(new RightElementViewModel()),
            "Turnout(L)"   => new TrackView(new LeftTurnoutViewModel()),
            "Turnout(R)"   => new TrackView(new RightTurnoutViewModel()),
            "Wye-Junction" => new TrackView(new YJunctionViewModel()),
            _              => new TrackView(new BlankElementViewModel())
        };
        elementView.ViewModel.Element = element;
        elementView.ViewModel.Element.Type = element.Type;
        return elementView;
    } 
    
    public static IElementView GetElementView(string elementName) {
        return GetElementView(new PanelElement() { Type = elementName });
    }
    
    
}