using CommunityToolkit.Maui.Views;

// For PopupOpenedEventArgs

// Ensure your IPropertiesViewModel namespace is correctly using'd
// e.g., using DCCPanelController.ViewModels.Properties; 

namespace DCCPanelController.View.Properties;

public partial class PropertyPopup : Popup {
    // Define margins and absolute minimum/maximum dimensions
    private const double ScreenMarginForPopup = 40;
    private const double MinPopupWidth = 500;
    private const double MinPopupHeight = 400;
    private readonly TaskCompletionSource<bool> _popupClosedTcs = new();

    //TODO: FIX
    //private bool _initialSizeLocked;

    public PropertyPopup(IPropertiesViewModel viewModel) {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
        PropertyViewContainer.Content = ViewModel.CreatePropertiesView();

        // TODO: FIX Opened += OnPopupOpened;
        // TODO: FIX Closed += OnPopupClosed;
    }

    public Task<bool> PopupClosedTask => _popupClosedTcs.Task;
    public IPropertiesViewModel ViewModel { get; }

    // TODO: FIX 
    // private void OnPopupOpened(object? sender, PopupOpenedEventArgs e) {
    //     // We only want to capture and lock the initial size once.
    //     if (!_initialSizeLocked && Content is Border popupContentBorder) {
    //         LockPopupToInitialSize(popupContentBorder);
    //         _initialSizeLocked = true;
    //     }
    // }

    // TODO: FIX 
    // private void OnPopupClosed(object? sender, PopupClosedEventArgs e) {
    //     Opened -= OnPopupOpened;
    //     Closed -= OnPopupClosed;
    //     _popupClosedTcs.TrySetResult(e.Result as bool? ?? false);
    // }

    private void LockPopupToInitialSize(Border popupContentBorder) {
        if (DeviceDisplay.Current?.MainDisplayInfo == null) return;

        var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;
        var screenWidthInDips = mainDisplayInfo.Width / mainDisplayInfo.Density;
        var screenHeightInDips = mainDisplayInfo.Height / mainDisplayInfo.Density;

        // Calculate maximum allowed dimensions, ensuring they are not less than our hardcoded minimums.
        var maxAllowedWidth = Math.Max(MinPopupWidth, screenWidthInDips - ScreenMarginForPopup);
        var maxAllowedHeight = Math.Max(MinPopupHeight, screenHeightInDips - ScreenMarginForPopup);

        // 1. Ensure WidthRequest/HeightRequest are initially unset to allow content to determine size.
        popupContentBorder.WidthRequest = -1;
        popupContentBorder.HeightRequest = -1;

        // 2. Apply Max constraints and absolute Min constraints.
        //    The popup will initially size itself based on content, within these bounds.
        popupContentBorder.MaximumWidthRequest = maxAllowedWidth;
        popupContentBorder.MaximumHeightRequest = maxAllowedHeight;
        popupContentBorder.MinimumWidthRequest = MinPopupWidth;   // Acts as an absolute floor
        popupContentBorder.MinimumHeightRequest = MinPopupHeight; // Acts as an absolute floor

        // 3. Defer reading dimensions and locking size until after the current layout pass
        //    and a slight delay to allow complex content (like an expander) to fully render.
        popupContentBorder.Dispatcher.Dispatch(async () => {
            // This delay helps ensure that the UI has had a chance to settle.
            // 100ms is a common starting point; adjust if needed (50-150ms).
            await Task.Delay(100);

            var currentActualWidth = popupContentBorder.Width;
            var currentActualHeight = popupContentBorder.Height;

            if (currentActualWidth <= 0 || currentActualHeight <= 0) {
                // Fallback to MinPopupWidth/Height if reading actual dimensions failed
                currentActualWidth = MinPopupWidth;
                currentActualHeight = MinPopupHeight;
            }

            // Clamp the captured actual size to be within our absolute minimums and screen-derived maximums.
            var lockedWidth = Math.Clamp(currentActualWidth, MinPopupWidth, maxAllowedWidth);
            var lockedHeight = Math.Clamp(currentActualHeight, MinPopupHeight, maxAllowedHeight);

            // 4. Fix the Border's size by setting its explicit WidthRequest and HeightRequest.
            //    This is the crucial step to prevent shrinking.
            popupContentBorder.WidthRequest = lockedWidth;
            popupContentBorder.HeightRequest = lockedHeight;

            // 5. As a reinforcement, also set the Minimum requests to these locked dimensions.
            //    This makes the "locked" size also the minimum size.
            popupContentBorder.MinimumWidthRequest = lockedWidth;
            popupContentBorder.MinimumHeightRequest = lockedHeight;

            Console.WriteLine($"PropertyPopup: Initial size captured and locked to Width={lockedWidth}, Height={lockedHeight}");
        });
    }

    private async void CloseButton_Clicked(object sender, EventArgs e) {
        // TODO: FIX 
        // await ViewModel.ApplyChangesAsync();
        // _popupClosedTcs.TrySetResult(true);
        // await CloseAsync(true);
    }

    // TODO: FIX 
    // protected override async Task OnDismissedByTappingOutsideOfPopup(CancellationToken token = new()) {
    //     // Consider if changes should be applied or discarded when tapping outside.
    //     // For now, assuming discard (or already handled by ViewModel if needed).
    //     // if (ViewModel != null) await ViewModel.ApplyChangesAsync(); 
    //     _popupClosedTcs.TrySetResult(false);
    //     await base.OnDismissedByTappingOutsideOfPopup(token);
    // }
}