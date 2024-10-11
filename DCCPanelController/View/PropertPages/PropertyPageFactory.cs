using System.Net.Mime;
using DCCPanelController.Helpers.Result;
using DCCPanelController.Model;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.View.PropertPages;

public class PropertyPageFactory {
    public static Result<IView> CreatePropertyPage(object model) {
        ArgumentNullException.ThrowIfNull(model);
        return model switch {
            Panel panel                   => Result<IView>.Success(new PanelPropertyPage(panel)),
            ButtonPanelElement button     => Result<IView>.Success(new ButtonPropertyPage(button)),
            CircleTextPanelElement circle => Result<IView>.Success(new CirclePropertyPage(circle)),
            ImagePanelElement image       => Result<IView>.Success(new ImagePropertyPage(image)),
            RoutePanelElement route       => Result<IView>.Success(new RoutePropertyPage(route)),
            TextPanelElement text         => Result<IView>.Success(new TextPropertyPage(text)),
            TrackPanelElement track       => Result<IView>.Success(new TrackPropertyPage(track)),
            TurnoutPanelElement turnout   => Result<IView>.Success(new TurnoutPropertyPage(turnout)),
            _                             => Result<IView>.Failure("Unsupported type")
        };
    }
}