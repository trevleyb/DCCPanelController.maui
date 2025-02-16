using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;
using OnScreenSizeMarkup.Maui.Helpers;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage, INotifyPropertyChanged {
    private readonly PanelsViewModel ViewModel;

    public PanelsPage() {
        ViewModel = new PanelsViewModel();
        BindingContext = ViewModel;
        InitializeComponent();
    }

    protected override void OnAppearing() {
        UpdateLayout();
        base.OnAppearing();
    }

    private void UpdateLayout() {
        var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
        var span = orientation switch {
            DisplayOrientation.Portrait  => OnScreenSizeHelpers.Instance.GetScreenSizeValue(1, 1, 1, 1, 1, 2),
            DisplayOrientation.Landscape => OnScreenSizeHelpers.Instance.GetScreenSizeValue(2, 2, 2, 2, 2, 3),
            _                            => OnScreenSizeHelpers.Instance.GetScreenSizeValue(1, 1, 1, 1, 1, 2)
        };

        PanelsCollectionViewLayout.Span = span;
        PanelsCollectionView.InvalidateMeasure();
    }

    private void Panel_OnDragStarting(object? sender, DragStartingEventArgs e) {
        Console.WriteLine("DragGestureRecognizer_OnDragStarting");
        if (sender is BindableObject { BindingContext: Panel panel }) {
            e.Data.Properties.Add("Panel", panel);
            e.Data.Properties.Add("Source", "Panel");
            e.Data.Properties.Add("Index", ViewModel.Panels.IndexOf(panel));
        }
    }

    private void Panel_OnDrop(object? sender, DropEventArgs e) {
        Console.WriteLine("DropGestureRecognizer_OnDrop");
        if (sender is BindableObject { BindingContext: Panel target }) {
            var source = e?.Data?.Properties["Source"] as string ?? null;
            var panel = e?.Data?.Properties["Panel"] as Panel ?? null;
            var index = e?.Data?.Properties["Index"] as int? ?? -1;
            if (source is not "Panel" || panel is null || index < 0) return;

            var newIndex = ViewModel.Panels.IndexOf(target);
            if (newIndex < 0) return;
            if (index < newIndex) newIndex--;

            ViewModel.Panels.RemoveAt(index);
            ViewModel.Panels.Insert(newIndex, panel);

            // ReApply the Sort Order so we order the list by this number
            // ------------------------------------------------------------
            for (var panelIndex = 0; panelIndex < ViewModel.Panels.Count; panelIndex++) {
                ViewModel.Panels[panelIndex].SortOrder = panelIndex + 1;
            }
        }
    }
}