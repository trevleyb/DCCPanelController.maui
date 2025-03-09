using DCCPanelController.Tests;

namespace DCCPanelController.View;

public partial class AboutPage : ContentPage {
    public AboutPage() {
        InitializeComponent();
    }

    private void Button_OnClicked(object? sender, EventArgs e) {
        var test = new TestDataModel();
        test.LoadAndSaveDataModel();
    }

    private void ToggleDesignMode(object? sender, EventArgs e) {
        if (BindingContext is AboutViewModel vm) {
            vm.DesignMode = !vm.DesignMode;
        }
    }

    private void ToggleShowGrid(object? sender, EventArgs e) {
        if (BindingContext is AboutViewModel vm) {
            vm.ShowGrid = !vm.ShowGrid;
        }
    }

    private void Slider_OnValueChanged(object? sender, ValueChangedEventArgs e) {
        if (BindingContext is AboutViewModel vm) {
            var panelSize = vm.Zoom * 10;
            PanelView.HeightRequest = panelSize;
            PanelView.WidthRequest = panelSize;
        }
    }
}