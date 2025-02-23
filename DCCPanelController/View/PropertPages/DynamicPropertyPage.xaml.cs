using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.ViewModel;
#if IOS
using UIKit;
#endif

namespace DCCPanelController.View.PropertPages;

public partial class DynamicPropertyPage : ContentPage, IPropertyPage {

    private ITrackPiece _trackPiece;
    private DynamicPropertyPageViewModel _viewModel;
    public DynamicPropertyPage(ITrackPiece trackPiece, string? propertyName = null) {
        _trackPiece = trackPiece;
        InitializeComponent();
        BindingContext = _viewModel = new DynamicPropertyPageViewModel(_trackPiece, propertyName);
        _viewModel.BuildProperties(PropertyContainer, _trackPiece);
    }

    public event EventHandler? CloseRequested;

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
        //if (_trackPiece is not null) _trackPiece.CleanUp(); 
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}