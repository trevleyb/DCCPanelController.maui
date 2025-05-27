using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core; // For PopupOpenedEventArgs
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

// Ensure your IPropertiesViewModel namespace is correctly using'd
// e.g., using DCCPanelController.ViewModels.Properties; 

namespace DCCPanelController.View.Properties {
    public partial class PropertyPopup : Popup {
        private readonly TaskCompletionSource<bool> _popupClosedTcs = new TaskCompletionSource<bool>();
        public Task<bool> PopupClosedTask => _popupClosedTcs.Task;
        public IPropertiesViewModel ViewModel { get; }

        // Define margins and absolute minimum/maximum dimensions
        private const double ScreenMarginForPopup = 40;
        private const double MinPopupWidth = 280;
        private const double MinPopupHeight = 300; // Adjusted slightly, can be tuned

        private bool _initialSizeLocked = false;

        public PropertyPopup(IPropertiesViewModel viewModel) {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
            PropertyViewContainer.Content = ViewModel.CreatePropertiesView();
            this.Opened += OnPopupOpened;
            this.Closed += OnPopupClosed;
        }

        private void OnPopupOpened(object? sender, PopupOpenedEventArgs e) {
            // We only want to capture and lock the initial size once.
            if (!_initialSizeLocked && this.Content is Border popupContentBorder) {
                LockPopupToInitialSize(popupContentBorder);
                _initialSizeLocked = true;
            }
        }
        
        private void OnPopupClosed(object? sender, PopupClosedEventArgs e) {
            this.Opened -= OnPopupOpened;
            this.Closed -= OnPopupClosed;
            _popupClosedTcs.TrySetResult(e.Result as bool? ?? false);
        }
        
        private void LockPopupToInitialSize(Border popupContentBorder) {
            if (DeviceDisplay.Current?.MainDisplayInfo == null) return;

            var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;
            double screenWidthInDips = mainDisplayInfo.Width / mainDisplayInfo.Density;
            double screenHeightInDips = mainDisplayInfo.Height / mainDisplayInfo.Density;

            // Calculate maximum allowed dimensions, ensuring they are not less than our hardcoded minimums.
            double maxAllowedWidth = Math.Max(MinPopupWidth, screenWidthInDips - ScreenMarginForPopup);
            double maxAllowedHeight = Math.Max(MinPopupHeight, screenHeightInDips - ScreenMarginForPopup);

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

                double currentActualWidth = popupContentBorder.Width;
                double currentActualHeight = popupContentBorder.Height;

                if (currentActualWidth <= 0 || currentActualHeight <= 0) {
                    Console.WriteLine($"PropertyPopup: Warning - Read invalid actual dimensions ({currentActualWidth}x{currentActualHeight}). Falling back to MinPopup constants to lock size.");

                    // Fallback to MinPopupWidth/Height if reading actual dimensions failed
                    currentActualWidth = MinPopupWidth;
                    currentActualHeight = MinPopupHeight;
                }

                // Clamp the captured actual size to be within our absolute minimums and screen-derived maximums.
                double lockedWidth = Math.Clamp(currentActualWidth, MinPopupWidth, maxAllowedWidth);
                double lockedHeight = Math.Clamp(currentActualHeight, MinPopupHeight, maxAllowedHeight);

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
            await ViewModel.ApplyChangesAsync();
            _popupClosedTcs.TrySetResult(true);
            await CloseAsync(true);
        }

        protected override async Task OnDismissedByTappingOutsideOfPopup(CancellationToken token = new CancellationToken()) {
            // Consider if changes should be applied or discarded when tapping outside.
            // For now, assuming discard (or already handled by ViewModel if needed).
            // if (ViewModel != null) await ViewModel.ApplyChangesAsync(); 
            _popupClosedTcs.TrySetResult(false);
            await base.OnDismissedByTappingOutsideOfPopup(token);
        }
    }
}