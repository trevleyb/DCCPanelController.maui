namespace DCCPanelController.View;

public partial class OperatePage : ContentPage {
    public OperatePage() {
        InitializeComponent();
    }

    private void ButtonAbout_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }
    private void ButtonInstructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }
}