// Assuming IPropertiesViewModel is here

namespace DCCPanelController.View.Properties {
    public partial class PropertySheetPage : ContentPage {
        private readonly TaskCompletionSource<bool> _pageClosedTcs = new TaskCompletionSource<bool>();
        public Task<bool> PageClosedTask => _pageClosedTcs.Task;
        public IPropertiesViewModel ViewModel { get; }

        public PropertySheetPage(IPropertiesViewModel viewModel) {
            Console.WriteLine($"PropertySheetPage: Initialising");
            InitializeComponent();
            Console.WriteLine($"PropertySheetPage: Initialised");
            ViewModel = viewModel;
            BindingContext = ViewModel; // To bind to ViewModel.Title in XAML's Page Title
            PropertyViewContainer.Content = ViewModel.CreatePropertiesView();
            Console.WriteLine($"PropertySheetPage: Constructor Exited");
        }

        private async Task HandleClosingActionsAsync() {
            Console.WriteLine($"PropertySheetPage: HandleClosingActions");
            await ViewModel.ApplyChangesAsync();
            _pageClosedTcs.TrySetResult(true);
        }

        protected override async void OnDisappearing() {
            Console.WriteLine($"PropertySheetPage: OnDisappearing");
            base.OnDisappearing();

            // This ensures actions are taken if dismissed by gesture (iOS PageSheet)
            if (!_pageClosedTcs.Task.IsCompleted) {
                await HandleClosingActionsAsync();
            }
        }

        private async void DoneButton_Clicked(object sender, EventArgs e) {
            Console.WriteLine($"PropertySheetPage: Done Button Pressed");
            if (!_pageClosedTcs.Task.IsCompleted) { // Prevent double execution
                await HandleClosingActionsAsync();
            }

            if (Navigation.ModalStack.Contains(this)) {
                await Navigation.PopModalAsync();
            }
        }
    }
}