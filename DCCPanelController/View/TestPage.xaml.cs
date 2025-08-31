using DCCPanelController.View.TileSelectors;

namespace DCCPanelController.View;

public partial class TestPage : ContentPage {
    private double _panStartX;
    private double _panStartY;

    public TestPage(TestPageViewModel viewModel) {
        InitializeComponent();
        BindingContext = viewModel;

        // PillSelectorContainer.Panel = viewModel.Panel;
        // SideSelectorContainer.Panel = viewModel.Panel;
    }

    private void PillSelectorContainer_OnOnDockSideChanged(object? sender, TileSelectorDockSide e) {
        // PillSelectorContainer.IsVisible = false;
        // SideSelectorContainer.IsVisible = true;
    }

    private void SideSelectorContainer_OnOnDockSideChanged(object? sender, TileSelectorDockSide e) {
        // PillSelectorContainer.IsVisible = true;
        // SideSelectorContainer.IsVisible = false;
    }

    private void TapGestureRecognizer_OnTapped1(object? sender, TappedEventArgs e) {
        Console.WriteLine("Tapped x1");
    }

    private void TapGestureRecognizer_OnTapped2(object? sender, TappedEventArgs e) {
        Console.WriteLine("Tapped x2");
    }

    private void DynamicGridPanUpdated(object? sender, PanUpdatedEventArgs e) {
        switch (e.StatusType) {
        case GestureStatus.Started:
            _panStartX = e.TotalX;
            _panStartY = e.TotalY;
            Console.WriteLine($"Pan Started: {_panStartX},{_panStartY}");
            break;

        case GestureStatus.Running:
            Console.WriteLine($"Pan Running: {e.TotalX},{e.TotalY}");
            break;

        case GestureStatus.Completed:
            Console.WriteLine($"Pan Ended: {_panStartX},{_panStartY}");
            break;

        case GestureStatus.Canceled:
            Console.WriteLine($"Pan Cancelled: {_panStartX},{_panStartY}");
            break;
        }
    }
}