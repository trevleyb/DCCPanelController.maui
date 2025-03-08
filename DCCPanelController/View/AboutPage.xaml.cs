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
}