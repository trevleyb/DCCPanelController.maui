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
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();

        builder.Services.AddSingleton<StorageService> ();
        builder.Services.AddSingleton<OperateService> ();
        
        builder.Services.AddSingleton<PanelsPage>();
        builder.Services.AddSingleton<PanelsService> ();
        builder.Services.AddTransient<PanelsDetailsService> ();

        builder.Services.AddSingleton<PanelDetailsPage>();
        builder.Services.AddSingleton<PanelsDetailsService>();
        builder.Services.AddSingleton<PanelsDetailsViewModel>();

        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<SettingsService> ();
        builder.Services.AddSingleton<SettingsViewModel> ();

        builder.Services.AddSingleton<RoutesPage>();
        builder.Services.AddSingleton<RoutesViewModel> ();
        builder.Services.AddSingleton<RoutesService> ();

        builder.Services.AddSingleton<TurnoutsPage>();
        builder.Services.AddSingleton<TurnoutStateViewModel> ();
        builder.Services.AddSingleton<TurnoutStateService> ();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        var app = builder.Build();
        // Assign the service provider to a static property
        App.ServiceProvider = app.Services;
        return app;
    }
}
