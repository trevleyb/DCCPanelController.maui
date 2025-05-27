// Assuming IPropertiesViewModel is here
using YourAppNamespace.ViewModels;

namespace DCCPanelController.View.Properties {
    public partial class PropertySheetPage : ContentPage {
        private readonly TaskCompletionSource<bool> _pageClosedTcs = new TaskCompletionSource<bool>();
        public Task<bool> PageClosedTask => _pageClosedTcs.Task;
        public IPropertiesViewModel ViewModel { get; }

        public PropertySheetPage(IPropertiesViewModel viewModel) {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = this; // To bind to ViewModel.Title in XAML's Page Title
            PropertyViewContainer.Content = ViewModel.CreatePropertiesView();
        }

        private async Task HandleClosingActionsAsync() {
            await ViewModel.ApplyChangesAsync();
            _pageClosedTcs.TrySetResult(true);
        }

        protected override async void OnDisappearing() {
            base.OnDisappearing();

            // This ensures actions are taken if dismissed by gesture (iOS PageSheet)
            if (!_pageClosedTcs.Task.IsCompleted) {
                await HandleClosingActionsAsync();
            }
        }

        private async void DoneButton_Clicked(object sender, EventArgs e) {
            if (!_pageClosedTcs.Task.IsCompleted) { // Prevent double execution
                await HandleClosingActionsAsync();
            }

            if (Navigation.ModalStack.Contains(this)) {
                await Navigation.PopModalAsync();
            }
        }
    }
}