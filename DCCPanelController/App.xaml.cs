using DCCPanelController.View;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace DCCPanelController;

public partial class App : Application {
    public App() {
        InitializeComponent();
        BindingDiagnostics.BindingFailed += BindingDiagnosticsOnBindingFailed;
    }

    /// <summary>
    ///     Gets the current <see cref="App" /> instance in use
    /// </summary>
    public new static App Current => Application.Current as App ?? throw new InvalidOperationException("Current application is not an instance of App");

    private void BindingDiagnosticsOnBindingFailed(object? sender, BindingBaseErrorEventArgs e) {
        Console.WriteLine("Binding Failed: " + (e?.XamlSourceInfo?.SourceUri.ToString() ?? "?SourceURI") + " | " + (e?.XamlSourceInfo?.LineNumber.ToString() ?? "?LineNum") + " | " + (e?.Binding?.ToString() ?? "?Binding") + " | " + (e?.Message ?? "?Message") + " | " + (e?.Binding?.GetType().Name ?? "?BindingType"));
    }

    protected override Window CreateWindow(IActivationState? activationState) {
        return new Window(new MainPageTabbed());
    }
}