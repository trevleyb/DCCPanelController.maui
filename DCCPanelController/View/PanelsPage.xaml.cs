using System.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;
using OnScreenSizeMarkup.Maui.Helpers;
using StackExchange.Profiling;

namespace DCCPanelController.View;

public partial class PanelsPage : ContentPage, INotifyPropertyChanged {

    public PanelsPage() {
        BindingContext = new PanelsViewModel();
        InitializeComponent();
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        MiniProfiler.Configure(MiniProfiler.DefaultOptions);
        MiniProfiler.StartNew("Panels Viewer");
        MiniProfiler.Current.Step("PanelsPage");
    }

    protected override void OnDisappearing() {
        Console.WriteLine(MiniProfiler.Current?.RenderPlainText());
        MiniProfiler.Current?.Stop();
        base.OnDisappearing();
    }

    private void UpdateLayout() {
        var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
        var span = orientation switch {
            DisplayOrientation.Portrait  => OnScreenSizeHelpers.Instance.GetScreenSizeValue(1, 1, 1, 1, 1, 2),
            DisplayOrientation.Landscape => OnScreenSizeHelpers.Instance.GetScreenSizeValue(2, 2, 2, 2, 2, 3),
            _                            => OnScreenSizeHelpers.Instance.GetScreenSizeValue(1, 1, 1, 1, 1, 2),
        };
        PanelsCollectionViewLayout.Span = span;
    }

    private void GoToSelectedPanelEditor(object? sender, TappedEventArgs e) {
        if (sender is BindableObject { BindingContext: Panel panel }) {
            LaunchPanelEditor(panel);
        }
    }

    /// <summary>
    /// Launch the Panel Editor passing through the selected panel to be edited. 
    /// </summary>
    /// <param name="panel">The current seleced Panel</param>
    private async Task LaunchPanelEditor(Panel panel) {
        Console.WriteLine($"Stopping here to check out the panel {panel.Name}");
        try {
            var editorPage = new PanelEditorPage(panel);
            //editorPage.OnFinished += EditorPageOnOnFinished;
            await this.Navigation.PushAsync(editorPage);
        } catch (Exception ex) {
            Console.WriteLine($"Failed to goto the Panel details for {panel.Name} due to {ex.Message}");
        }
    }
}