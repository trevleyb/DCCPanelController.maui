// Assuming IPropertiesViewModel is here
// For PropertySheetPage and PropertyPopup

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace DCCPanelController.View.Properties;

public static class PropertyDisplayService {
    public enum ShowPropertiesType { Automatic, PropertySheet, PopUpWindow }

    public static async Task<bool> ShowPropertiesAsync(INavigation navigation,
                                                       IPropertiesViewModel viewModel,
                                                       double currentDisplayWidth,  // Pass the current display width
                                                       double currentDisplayHeight, // Pass the current display height
                                                       ShowPropertiesType showPropertiesType = ShowPropertiesType.Automatic) {
        var result = false;

        // Determine if it's an iPhone-like device in portrait
        var usePageSheet = DeviceInfo.Platform == DevicePlatform.iOS; 
                         //&& DeviceInfo.Current.Idiom == DeviceIdiom.Phone;

        //currentDisplayWidth < currentDisplayHeight; // Basic portrait check

        if (usePageSheet && showPropertiesType == ShowPropertiesType.Automatic || showPropertiesType == ShowPropertiesType.PropertySheet) {
            var propertySheetPage = new PropertySheetPage(viewModel);
#if IOS
            propertySheetPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
#endif
            await navigation.PushModalAsync(propertySheetPage);
            result = await propertySheetPage.PageClosedTask;
            Console.WriteLine($"PropertyDisplayService:PageSheet Closed with Result={result}");
        } else {
            // For iPad, Mac, Windows, Android tablets, or landscape iPhone
            var propertyPopup = new PropertyPopup(viewModel);

            // The PropertyPopup.ShowAsync handles showing and awaiting closure
            // This is a simplified call. You might need to pass the current page.
            if (App.Current.Windows[0].Page is { } mainPage) {
                await mainPage.ShowPopupAsync(propertyPopup);
                result = await propertyPopup.PopupClosedTask;
                propertyPopup?.Close();
                Console.WriteLine($"PropertyDisplayService:Popup Closed with Result={result}");
            } else {
                // Fallback or error handling if MainPage is not available
                // For simplicity, we'll assume MainPage is available.
                // In a real app, you might need more robust handling.
                Console.WriteLine("PropertyDisplayService: Error: MainPage not available for showing popup.");
                return false; // Or throw an exception
            }
        }
        return result;
    }
}