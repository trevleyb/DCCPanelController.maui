using DCCPanelController.Tests;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.Properties;

namespace DCCPanelController.View;

public partial class TestPanelPage : ContentPage {
    public TestPanelPage() {
        InitializeComponent();
        BindingContext = new TestPanelPageModel();
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
        var panelViewModel = new Properties.Testing.MyPanelViewModel("Test Panel Name");
        bool result = await PropertyDisplayService.ShowPropertiesAsync(
            Navigation,
            panelViewModel,
            Width,
            Height, PropertyDisplayService.ShowPropertiesType.PropertySheet);

        if (result) {
            // Properties were applied and closed (e.g., "Done" or "Close" was hit)
            System.Diagnostics.Debug.WriteLine("Properties applied successfully.");
        } else {
            // Properties dialog was dismissed without explicit apply (e.g., tap outside popup)
            System.Diagnostics.Debug.WriteLine("Properties view dismissed.");
        }
    }

    private async void Popup_Button_OnClicked(object? sender, EventArgs e) {
        var panelViewModel = new Properties.Testing.MyPanelViewModel("Test Panel Name");
        bool result = await PropertyDisplayService.ShowPropertiesAsync(
            Navigation,
            panelViewModel,
            Width,
            Height, PropertyDisplayService.ShowPropertiesType.PopUpWindow);

        if (result) {
            // Properties were applied and closed (e.g., "Done" or "Close" was hit)
            System.Diagnostics.Debug.WriteLine("Properties applied successfully.");
        } else {
            // Properties dialog was dismissed without explicit apply (e.g., tap outside popup)
            System.Diagnostics.Debug.WriteLine("Properties view dismissed.");
        }
    }
    
    private async void Auto_Button_OnClicked(object? sender, EventArgs e) {
        var panelViewModel = new Properties.Testing.MyPanelViewModel("Test Panel Name");
        bool result = await PropertyDisplayService.ShowPropertiesAsync(
            Navigation,
            panelViewModel,
            Width,
            Height, PropertyDisplayService.ShowPropertiesType.Automatic);

        if (result) {
            // Properties were applied and closed (e.g., "Done" or "Close" was hit)
            System.Diagnostics.Debug.WriteLine("Properties applied successfully.");
        } else {
            // Properties dialog was dismissed without explicit apply (e.g., tap outside popup)
            System.Diagnostics.Debug.WriteLine("Properties view dismissed.");
        }
    }

}