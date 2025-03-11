using DCCPanelController.Tests;
using DCCPanelController.View.Helpers;

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
}