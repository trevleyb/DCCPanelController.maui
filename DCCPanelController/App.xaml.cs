using DCCPanelController.View;

namespace DCCPanelController;

public partial class App : Application {
    public App() {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public new static App Current => Application.Current as App ?? throw new InvalidOperationException("Current application is not an instance of App");

    protected override Window CreateWindow(IActivationState? activationState) {
        return new Window(new MainPageTabbed());
    }
}