using DCCPanelController.Services;
using DCCPanelController.View.Helpers;
using Foundation;
using UIKit;
using ObjCRuntime;

namespace DCCPanelController;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate {
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}