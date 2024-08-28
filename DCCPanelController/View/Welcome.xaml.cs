
namespace DCCPanelController.View;

public partial class Welcome : ContentPage {
    public Welcome() {
        InitializeComponent();
    }

    private void TrackTest_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new TrackPieceTestPage());
    }
    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }
    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }
}