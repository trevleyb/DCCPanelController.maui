using CommunityToolkit.Maui.Views;

// Assuming IPropertiesViewModel is here

namespace DCCPanelController.View.Properties {
    public partial class PropertyPopup : Popup {
        private readonly TaskCompletionSource<bool> _popupClosedTcs = new TaskCompletionSource<bool>();
        public Task<bool> PopupClosedTask => _popupClosedTcs.Task;
        public IPropertiesViewModel ViewModel { get; }

        // Define margins and minimum/maximum dimensions
        private const double ScreenMarginForPopup = 40; // Total margin (e.g., 20px on each of the 4 sides from screen edge)
        private const double MinPopupWidth = 280;       // Minimum sensible width for the popup
        private const double MinPopupHeight = 400;      // Minimum sensible height for the popup
        
        public PropertyPopup(IPropertiesViewModel viewModel) {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel; // To bind to ViewModel.Title in XAML
            PropertyViewContainer.Content = ViewModel.CreatePropertiesView();
            this.Opened += OnPopupOpened;
        }
        
        private void OnPopupOpened(object? sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e) {
            UpdatePopupSizeConstraints();
        }

        private void UpdatePopupSizeConstraints() {
            if (DeviceDisplay.Current?.MainDisplayInfo == null) return;

            var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;
            double screenWidthInDips = mainDisplayInfo.Width / mainDisplayInfo.Density;
            double screenHeightInDips = mainDisplayInfo.Height / mainDisplayInfo.Density;

            // Calculate maximum allowed dimensions considering the margin
            var maxAllowedWidth  = screenWidthInDips - ScreenMarginForPopup;
            var maxAllowedHeight = screenHeightInDips - ScreenMarginForPopup;

            // Ensure calculated max dimensions are not less than min dimensions, and not negative
            maxAllowedWidth = Math.Max(MinPopupWidth, maxAllowedWidth);
            maxAllowedHeight = Math.Max(MinPopupHeight, maxAllowedHeight);
            
            // The Popup's direct child is the Border (named PopupBorder in XAML)
            if (this.Content is Border popupContentBorder) { // this.Content refers to the direct child of the Popup
                // Unset any explicitly set size requests first to allow natural sizing up to the maximum.
                popupContentBorder.WidthRequest = -1; // Equivalent to Double.NaN for MAUI's auto-sizing
                popupContentBorder.HeightRequest = -1;

                popupContentBorder.MaximumWidthRequest = maxAllowedWidth;
                popupContentBorder.MaximumHeightRequest = maxAllowedHeight;

                popupContentBorder.MinimumWidthRequest = MinPopupWidth;
                popupContentBorder.MinimumHeightRequest = MinPopupHeight;
            }
        }

        private async void CloseButton_Clicked(object sender, EventArgs e) {
            await ViewModel.ApplyChangesAsync();
            _popupClosedTcs.TrySetResult(true);
            await CloseAsync(true); // Close the popup and return 'true'
        }

        // Override OnDismissedByTappingOutside if you need to apply changes
        // when the popup is dismissed by tapping outside.
        protected override async Task OnDismissedByTappingOutsideOfPopup(CancellationToken token = new CancellationToken()) {
            await ViewModel.ApplyChangesAsync(); 
            _popupClosedTcs.TrySetResult(false); // Indicate closure, maybe with 'false' if not saved
            base.OnDismissedByTappingOutsideOfPopup(token);
        }

        // This method allows the caller to await the popup's result
        public async Task<object?> ShowAsync(Page? page = null) {
            if (App.Current.Windows[0].Page is { } mainPage) {
                mainPage.ShowPopup(this);
                return await PopupClosedTask; // Wait for the task from _popupClosedTcs
            }
            return null;
        }
    }
}