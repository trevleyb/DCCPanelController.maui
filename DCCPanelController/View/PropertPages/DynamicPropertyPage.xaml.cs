using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.ViewModel;
#if IOS
using UIKit;
#endif

namespace DCCPanelController.View.PropertPages;

public partial class DynamicPropertyPage : ContentPage, IPropertyPage {

    public event EventHandler? CloseRequested;

    public DynamicPropertyPage(ITrackPiece obj, string? propertyName = null) {
        InitializeComponent();
        BindingContext = new DynamicPropertyPageViewModel(obj, propertyName, PropertyContainer);
    }

    protected override void OnAppearing() {
        base.OnAppearing();

#if IOS
        // Access the native view controller
        var window = App.Current.Windows[0].Page;
        if (window?.Handler?.PlatformView is UIWindow viewController) {
            var rootController = viewController.RootViewController;
            if (rootController?.PresentedViewController != null) {
                var modalController = rootController.PresentedViewController;
                modalController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
                if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0)) {
                    var sheetController = modalController.SheetPresentationController;
                    if (sheetController != null) {
                        sheetController.Detents = [
                            UISheetPresentationControllerDetent.CreateMediumDetent(),
                            UISheetPresentationControllerDetent.CreateLargeDetent()
                        ];
                        sheetController.PrefersGrabberVisible = true;
                    }
                }
            }
        }
#endif
    }
    
    private void ClosePropertyPage(object? sender, EventArgs? e) {
        //Navigation.PopModalAsync(true);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}