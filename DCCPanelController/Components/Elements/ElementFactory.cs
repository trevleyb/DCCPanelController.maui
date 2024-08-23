using System.Reflection;
using DCCPanelController.Components.Elements.ViewModels;
using DCCPanelController.Components.Elements.Views;
using DCCPanelController.Components.TrackComponents;
using DCCPanelController.Model.Elements;
using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Components.Elements;

public static class ElementFactory {

    /// <summary>
    /// Returns a View object thart can be placed on the UI/Panel to represent an Element
    /// </summary>
    /// <param name="element">The element associated with this View</param>
    /// <returns>A IElementView which is also a IView</returns>
    public static IElementView? CreateElementView(IPanelElement element) {
        return CreateElementView(element.SymbolType, element);
    }

    /// <summary>
    /// Creates the View using the Symbol Key (Set:Name) which also defines what the view model is
    /// </summary>
    /// <param name="elementKey">The key for the Element (Set:Name)</param>
    /// <param name="element">The element that this view will associate with. If blank, creates a new one</param>
    /// <returns>A IElementView whcih is also a IView</returns>
    public static IElementView? CreateElementView(string elementKey, IPanelElement? element = null) {
        var details = SymbolLoader.Symbols.GetByKey(elementKey);
        if (details is not null && !string.IsNullOrEmpty(details.ViewModel)) {
            IElementView? view = details.ViewModel.ToLower() switch {
                "track"      => new TrackElementView(new TrackElementViewModel((TrackPanelElement)(element ?? new TrackPanelElement()), details)),
                "text"       => new TextElementView(new TextElementViewModel((TextPanelElement)(element ?? new TextPanelElement()), details)),
                "image"      => new ImageElementView(new ImageElementViewModel((ImagePanelElement)(element ?? new ImagePanelElement()), details)),
                "turnout"    => new TurnoutElementView(new TurnoutElementViewModel((TurnoutPanelElement)(element ?? new TurnoutPanelElement()), details)),
                "route"      => new RouteElementView(new RouteElementViewModel((RoutePanelElement)(element ?? new RoutePanelElement()), details)),
                "button"     => new ButtonElementView(new ButtonElementViewModel((ButtonPanelElement)(element ?? new ButtonPanelElement()), details)),
                "circletext" => new CircleTextElementView(new TextElementViewModel((TextPanelElement)(element ?? new TextPanelElement()), details)),
                _            => throw new InvalidOperationException($"Unknown element type: {details.ViewModel}")
            };
            return view;
        }
        
        return null;
    }

    /// <summary>
    /// Uses reflection to find and create the ViewModel that will be associated with this element tyoe. 
    /// </summary>
    /// <param name="viewModelName">The name of the ViewModel to use</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If the ViewModel name is blank</exception>
    /// <exception cref="InvalidOperationException">If the ViewModel cannot be found in the assembly</exception>
    private static IElementViewModel? CreateViewModel(string viewModelName) {
        if (string.IsNullOrEmpty(viewModelName)) {
            throw new ArgumentException("ViewModel name cannot be null or empty.", nameof(viewModelName));
        }
        var assembly = Assembly.GetExecutingAssembly();
        var viewModelType = assembly.GetTypes().FirstOrDefault(x => x.FullName != null && x.FullName.Contains(viewModelName, StringComparison.OrdinalIgnoreCase));
        if (viewModelType == null)  {
            throw new InvalidOperationException($"Type {viewModelName} not found in assembly {assembly.FullName}.");
        }
        return (IElementViewModel)Activator.CreateInstance(viewModelType)! ?? null;
    }
}