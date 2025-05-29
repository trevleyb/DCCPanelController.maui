using DCCPanelController.Models.DataModel;
using DCCPanelController.Tests;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties;
using DCCPanelController.View.Properties.PanelProperties;

namespace DCCPanelController.View;

public partial class TestPanelPage : ContentPage {
    private readonly Panel panel;

    public TestPanelPage() {
        InitializeComponent();
        BindingContext = new TestPanelPageModel();

        var panels = new Panels();
        panel = panels.CreatePanel();
        panel.Id = "Test Panel";
    }

    protected override void OnBindingContextChanged() {
        base.OnBindingContextChanged();
        if (BindingContext is TestPanelPageModel vm) {
            DesignModeToolbar.IconImageSource = vm.DesignMode ? "play.png" : "edit.png";
            EditModeToolbar.IconImageSource = vm.EditMode == EditModeEnum.Move ? "move.png" : "copy.png";
            EditModeToolbar.IsEnabled = vm.DesignMode;
            OnPropertyChanged();
        }
    }

    private void Button_OnClicked(object? sender, EventArgs e) {
        var test = new TestDataModel();
        test.LoadAndSaveDataModel();
    }

    private void ToggleDesignMode(object? sender, EventArgs e) {
        if (BindingContext is TestPanelPageModel vm) {
            vm.DesignMode = !vm.DesignMode;
            DesignModeToolbar.IconImageSource = vm.DesignMode ? "play.png" : "edit.png";
            EditModeToolbar.IsEnabled = vm.DesignMode;
            OnPropertyChanged(nameof(DesignMode));
        }
    }

    private void ToggleEditMode(object? sender, EventArgs e) {
        if (BindingContext is TestPanelPageModel vm) {
            vm.EditMode = vm.EditMode == EditModeEnum.Move ? EditModeEnum.Copy : EditModeEnum.Move;
            EditModeToolbar.IconImageSource = vm.EditMode == EditModeEnum.Move ? "move.png" : "copy.png";
            EditModeToolbar.IsEnabled = vm.DesignMode;
            OnPropertyChanged(nameof(DesignMode));
        }
    }

    private async void Prop_Button_OnClicked(object? sender, EventArgs e) {
        var panelViewModel = new PanelPropertyViewModel(panel);
        var result = await PropertyDisplayService.ShowPropertiesAsync(
            Navigation,
            panelViewModel,
            Width,
            Height, PropertyDisplayService.ShowPropertiesType.PropertySheet);

        if (result) {
            // Properties were applied and closed (e.g., "Done" or "Close" was hit)
            Console.WriteLine("Properties applied successfully.");
        } else {
            // Properties dialog was dismissed without explicit apply (e.g., tap outside popup)
            Console.WriteLine("Properties view dismissed.");
        }
    }

    private async void Popup_Button_OnClicked(object? sender, EventArgs e) {
        var panelViewModel = new PanelPropertyViewModel(panel);
        var result = await PropertyDisplayService.ShowPropertiesAsync(
            Navigation,
            panelViewModel,
            Width,
            Height, PropertyDisplayService.ShowPropertiesType.PopUpWindow);

        if (result) {
            // Properties were applied and closed (e.g., "Done" or "Close" was hit)
            Console.WriteLine("Properties applied successfully.");
        } else {
            // Properties dialog was dismissed without explicit apply (e.g., tap outside popup)
            Console.WriteLine("Properties view dismissed.");
        }
    }

    private async void Auto_Button_OnClicked(object? sender, EventArgs e) {
        var panelViewModel = new PanelPropertyViewModel(panel);
        var result = await PropertyDisplayService.ShowPropertiesAsync(
            Navigation,
            panelViewModel,
            Width,
            Height);

        if (result) {
            // Properties were applied and closed (e.g., "Done" or "Close" was hit)
            Console.WriteLine("Properties applied successfully.");
        } else {
            // Properties dialog was dismissed without explicit apply (e.g., tap outside popup)
            Console.WriteLine("Properties view dismissed.");
        }
    }
}