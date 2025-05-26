using CommunityToolkit.Maui.Views;
using DCCPanelController.Models.DataModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace DCCPanelController.View.PanelProperties;

public static class PropertyPageLauncher {
    public static async Task ShowPanelPropertyPageAsync(Panel panel, INavigation navigation, double width, double height) {
        //if (DeviceInfo.Platform == DevicePlatform.iOS) {
            //if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone) {
            // On iPhone or a small device, so use a Navigation
            var navPage = new PanelPropertyPage(panel);
#if IOS
            navPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
#endif
            await navigation.PushModalAsync(navPage);
            await navPage.PageClosed;
            return;

            //}
        //}

        // Default otherwise to a popup window
        if (App.Current.Windows[0].Page is { } mainPage) {
            var popPage = new PanelPropertyPopup(panel);
            await mainPage.ShowPopupAsync(popPage);

            //await popPage.PageClosed;
        }
        return;
    }

    [Obsolete]
    public static async Task ShowPanelPropertyPageAsync(Panel panel) {
        // -------------------------------------------------------------------------------
        if (DeviceInfo.Idiom == DeviceIdiom.Phone && DeviceInfo.Platform == DevicePlatform.iOS) {
            await LaunchNavigationPage(new PanelPropertyPage(panel));
        } else if (DeviceInfo.Idiom == DeviceIdiom.Tablet && DeviceInfo.Platform == DevicePlatform.iOS ||
                   DeviceInfo.Platform == DevicePlatform.MacCatalyst) {
            await LaunchPopupPage(new PanelPropertyPopup(panel));
        }
    }

    private static async Task LaunchNavigationPage(ContentPage page) {
        if (App.Current.Windows[0].Page is { } mainPage) {
            var navigationPage = new NavigationPage(page);
#if IOS
            navigationPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
#endif
            await mainPage.Navigation.PushModalAsync(navigationPage);
        }
    }

    private static async Task LaunchPopupPage(Popup page) {
        if (App.Current.Windows[0].Page is { } mainPage) {
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet || DeviceInfo.Platform == DevicePlatform.MacCatalyst) {
                // popupPage.SetPopupSize(400, 600); // Example width/height in pixels
            }
            await mainPage.ShowPopupAsync(page);
        }
    }
}