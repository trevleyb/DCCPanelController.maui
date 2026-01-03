using DCCPanelController.Helpers;
using DCCPanelController.View.Helpers;
using Syncfusion.Maui.Toolkit.Popup;

namespace DCCPanelController.View;

public partial class AccessoriesPage : ContentPage {
    public AccessoriesPage() {
        BindingContext = this;
        InitializeComponent();
    }

    private void OnTurnoutsClicked(object? sender, EventArgs e) {
        SwitchToTab("Turnouts");
    }

    private void OnRoutesClicked(object? sender, EventArgs e) {
        SwitchToTab("Routes");
    }

    private void OnBlocksClicked(object? sender, EventArgs e) {
        SwitchToTab("Blocks");
    }

    private void OnLightsClicked(object? sender, EventArgs e) {
        SwitchToTab("Lights");
    }

    private void SwitchToTab(string tabTitle) {
        var accessoriesTab = Shell.Current.Items
            .OfType<FlyoutItem>()
            .FirstOrDefault()?
            .Items.OfType<Tab>()
            .FirstOrDefault(t => t.Title == "Accessories");

        if (accessoriesTab != null) {
            var targetShellContent = accessoriesTab.Items
                .OfType<ShellContent>()
                .FirstOrDefault(sc => sc.Title == tabTitle);

            if (targetShellContent != null) {
                Shell.Current.CurrentItem = accessoriesTab;
                accessoriesTab.CurrentItem = targetShellContent;
            }
        }
    }
}