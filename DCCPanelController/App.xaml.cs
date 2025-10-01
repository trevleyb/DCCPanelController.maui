using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Logging;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace DCCPanelController;

public partial class App : Application {
    public App(ProfileService profileService, IServiceProvider serviceProvider) {
        InitializeComponent();
        BindingDiagnostics.BindingFailed += BindingDiagnosticsOnBindingFailed;
    }

    /// <summary>
    ///     Gets the current <see cref="App" /> instance in use
    /// </summary>
    public new static App Current => Application.Current as App ?? throw new InvalidOperationException("Current application is not an instance of App");

    private static void BindingDiagnosticsOnBindingFailed(object? sender, BindingBaseErrorEventArgs e) {
        var logger = LogHelper.CreateLogger("BindingDiagnostics");
        logger.LogWarning("Binding Failed: {BindingSource} | {BindingLine} | {BindingName} | {BindingMessage} | {BindingType}",
            e?.XamlSourceInfo?.SourceUri.ToString() ?? "?SourceURI",
            e?.XamlSourceInfo?.LineNumber.ToString() ?? "?LineNum",
            e?.Binding?.ToString() ?? "?Binding",
            e?.Message ?? "?Message",
            e?.Binding?.GetType().Name ?? "?BindingType");
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(new LoadingPage());
}