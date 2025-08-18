// // Assuming IPropertiesViewModel is here
// // For PropertySheetPage and PropertyPopup
//
// using CommunityToolkit.Maui;
// using CommunityToolkit.Maui.Extensions;
// using CommunityToolkit.Maui.Views;
// using Microsoft.Maui.Controls.PlatformConfiguration;
// using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
// using Syncfusion.Maui.Toolkit.NavigationDrawer;
//
// namespace DCCPanelController.View.Properties;
//
// public static class NavigationDrawerService {
//
//     public static void ShowDrawer(INavigation navigation,
//                                   Microsoft.Maui.Controls.View view,
//                                   double currentDisplayWidth,
//                                   double currentDisplayHeight) {
//
//         var navigationDrawer = new SfNavigationDrawer {
//             ContentView = view,
//             DrawerSettings = new DrawerSettings() {
//                 DrawerWidth = 200,
//                 DrawerFooterHeight = 200,
//                 Position = Position.Bottom,
//             }
//         };
//         
//         
//     }
// }
//
// public static class PropertyDisplayService {
//     public enum ShowPropertiesType { Automatic, PropertySheet, PopUpWindow }
//
//     public static async Task<bool> ShowPropertiesAsync(INavigation navigation,
//                                                        IPropertiesViewModel viewModel,
//                                                        double currentDisplayWidth,  // Pass the current display width
//                                                        double currentDisplayHeight, // Pass the current display height
//                                                        ShowPropertiesType showPropertiesType = ShowPropertiesType.Automatic) {
//         var result = false;
//
//         // Determine if it's an iPhone-like device in portrait
//         var usePageSheet = DeviceInfo.Platform == DevicePlatform.iOS ||
//                            DeviceInfo.Platform == DevicePlatform.MacCatalyst;
//                          //&& DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
//
//         //currentDisplayWidth < currentDisplayHeight; // Basic portrait check
//
//         if (usePageSheet && showPropertiesType == ShowPropertiesType.Automatic || showPropertiesType == ShowPropertiesType.PropertySheet) {
//             var propertySheetPage = new PropertySheetPage(viewModel);
// #if IOS
//             propertySheetPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.PageSheet);
// #endif
//             await navigation.PushModalAsync(propertySheetPage);
//             result = await propertySheetPage.PageClosedTask;
//         } else {
//             // For iPad, Mac, Windows, Android tablets, or landscape iPhone
//             var propertyPopup = new PropertyPopup(viewModel);
//
//             var services = App.Current?.Handler?.MauiContext?.Services;
//             var popupService = services?.GetService<IPopupService>();
//             if (popupService is null) {
//                 return false;
//             }
//             
//             // TODO FIX: Popup Service 
//             //await popupService.ShowPopupAsync(propertyPopup);
//             //result = await propertyPopup.PopupClosedTask;
//
//             
//             // The PropertyPopup.ShowAsync handles showing and awaiting closure
//             // This is a simplified call. You might need to pass the current page.
//             // if (App.Current.Windows[0].Page is { } mainPage) {
//             //     await mainPage.ShowPopupAsync(propertyPopup);
//             //     result = await propertyPopup.PopupClosedTask;
//             //     propertyPopup?.Close();
//             // } else {
//             //     // Fallback or error handling if MainPage is not available
//             //     // For simplicity, we'll assume MainPage is available.
//             //     // In a real app, you might need more robust handling.
//             //     return false; // Or throw an exception
//             // }
//         }
//         return result;
//     }
// }