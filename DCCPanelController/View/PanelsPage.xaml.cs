using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;
using OnScreenSizeMarkup.Maui.Helpers;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage, INotifyPropertyChanged {
    private PanelsViewModel _viewModel;

    public PanelsPage() {
        _viewModel = new PanelsViewModel();
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        if (_viewModel.SelectedPanel is not null) {
            Console.WriteLine($"On Appearing Selected Panel: {_viewModel.SelectedPanel.Name}");
            _viewModel.OnEditorPageFinished(_viewModel.SelectedPanel);
        }

        _viewModel.SelectedPanel = null;
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();
    }

    private void UpdateLayout() {
        var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
        var span = orientation switch {
            DisplayOrientation.Portrait  => OnScreenSizeHelpers.Instance.GetScreenSizeValue(1, 1, 1, 1, 1, 2),
            DisplayOrientation.Landscape => OnScreenSizeHelpers.Instance.GetScreenSizeValue(2, 2, 2, 2, 2, 3),
            _                            => OnScreenSizeHelpers.Instance.GetScreenSizeValue(1, 1, 1, 1, 1, 2)
        };

        PanelsCollectionViewLayout.Span = span;
    }

    private async void GoToSelectedPanelEditor(object? sender, TappedEventArgs e) {
        if (sender is BindableObject { BindingContext: Panel panel }) {
            await LaunchPanelEditor(panel);
        }
    }

    /// <summary>
    /// Launch the Panel Editor passing through the selected panel to be edited. 
    /// </summary>
    /// <param name="panel">The current seleced Panel</param>
    private async Task LaunchPanelEditor(Panel panel) {
        Console.WriteLine($"Stopping here to check out the panel {panel.Name}");
        try {
            _viewModel.SelectedPanel = panel;
            Console.WriteLine($"Launch Editor Selected Panel: {_viewModel.SelectedPanel.Name}");
            var editorPage = new PanelEditorPage(_viewModel);
            await Navigation.PushAsync(editorPage);
        } catch (Exception ex) {
            Console.WriteLine($"Failed to goto the Panel details for {panel.Name} due to {ex.Message}");
        }
    }
}