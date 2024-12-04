using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;
using OnScreenSizeMarkup.Maui.Helpers;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage, INotifyPropertyChanged {
    private readonly PanelsViewModel _viewModel;

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
        IsBusy = true;
        Console.WriteLine("GoToSelectedPanelEditor");
        if (sender is BindableObject { BindingContext: Panel panel }) {
            await LaunchPanelEditor(panel);
        }

        IsBusy = false;
    }

    /// <summary>
    ///     Launch the Panel Editor passing through the selected panel to be edited.
    /// </summary>
    /// <param name="panel">The current seleced Panel</param>
    private async Task LaunchPanelEditor(Panel panel) {
        Console.WriteLine($"Stopping here to check out the panel {panel.Name}");
        IsBusy = true;
        try {
            _viewModel.SelectedPanel = panel;
            Console.WriteLine($"Launch Editor Selected Panel: {_viewModel.SelectedPanel.Name}");
            var editorPage = new PanelEditorPage(_viewModel);
            await Navigation.PushAsync(editorPage);
        } catch (Exception ex) {
            Console.WriteLine($"Failed to goto the Panel details for {panel.Name} due to {ex.Message}");
        }

        IsBusy = false;
    }

    private void Panel_OnDragStarting(object? sender, DragStartingEventArgs e) {
        Console.WriteLine("DragGestureRecognizer_OnDragStarting");
        if (sender is BindableObject { BindingContext: Panel panel }) {
            e.Data.Properties.Add("Panel", panel);
            e.Data.Properties.Add("Source", "Panel");
            e.Data.Properties.Add("Index", _viewModel.Panels.IndexOf(panel));
        }
    }

    private void Panel_OnDrop(object? sender, DropEventArgs e) {
        Console.WriteLine("DropGestureRecognizer_OnDrop");

        if (sender is BindableObject { BindingContext: Panel target }) {
            var source = e?.Data?.Properties["Source"] as string ?? null;
            var panel = e?.Data?.Properties["Panel"] as Panel ?? null;
            var index = e?.Data?.Properties["Index"] as int? ?? -1;
            if (source is not "Panel" || panel is null || index < 0) return;

            var newIndex = _viewModel.Panels.IndexOf(target);
            if (newIndex < 0) return;
            if (index < newIndex) newIndex--;

            _viewModel.Panels.RemoveAt(index);
            _viewModel.Panels.Insert(newIndex, panel);

            // ReApply the Sort Order so we order the list by this number
            // ------------------------------------------------------------
            for (var panelIndex = 0; panelIndex < _viewModel.Panels.Count; panelIndex++) {
                _viewModel.Panels[panelIndex].SortOrder = panelIndex + 1;
            }
        }
    }
}

// [RelayCommand]
// public async Task DropAsync(Panel panel) {
//     Console.WriteLine($"Dropped {panel.SystemName}");
//     var droppedIndex = Panels.IndexOf(panel);
//
//     // Swap or rearrange items
//     if (_draggingIndex >= 0 && droppedIndex >= 0) {
//         var draggedItem = Panels[_draggingIndex];
//         Panels.Remove(draggedItem);
//         Panels.Insert(droppedIndex, draggedItem);
//
//         // ReApply the Sort Order so we order the list by this number
//         // ------------------------------------------------------------
//         for (var index = 0; index < Panels.Count; index++) {
//             Panels[index].SortOrder = index + 1;
//         }
//     }
// }