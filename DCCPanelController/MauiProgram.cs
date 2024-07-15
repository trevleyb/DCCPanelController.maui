using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using DCCPanelController.Services;
using DCCPanelController.View;
using DCCPanelController.ViewModel;

namespace DCCPanelController;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts => {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();

        builder.Services.AddSingleton<SettingsService> ();
        builder.Services.AddSingleton<ConnectionService>();
        builder.Services.AddSingleton<RoutesService> ();
        builder.Services.AddSingleton<TurnoutsService> ();
        
        builder.Services.AddSingleton<PanelsPage>();
        builder.Services.AddSingleton<PanelsViewModel>();

        builder.Services.AddTransient<PanelEditorPage>();
        builder.Services.AddTransient<PanelEditorViewModel> ();
        
        builder.Services.AddSingleton<RoutesPage>();
        builder.Services.AddSingleton<RoutesViewModel> ();

        builder.Services.AddSingleton<TurnoutsPage>();
        builder.Services.AddSingleton<TurnoutsViewModel> ();
        
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<SettingsViewModel> ();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        var app = builder.Build();
        // Assign the service provider to a static property
        App.ServiceProvider = app.Services;
        return app;
    }
}
