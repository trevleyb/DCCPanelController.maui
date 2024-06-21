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

        //builder.Services.AddSingleton<PanelsPage>();
        //builder.Services.AddSingleton<PanelPage>();
        //builder.Services.AddSingleton<PanelDetailsPage>();

        
        builder.Services.AddSingleton<PanelService> ();
        builder.Services.AddSingleton<PanelsService> ();
        builder.Services.AddTransient<PanelDetailsService> ();

        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<SettingsService> ();
        builder.Services.AddSingleton<SettingsViewModel> ();
        
        builder.Services.AddSingleton<TurnoutsPage>();
        builder.Services.AddSingleton<TurnoutStateViewModel> ();
        builder.Services.AddSingleton<TurnoutStateService> ();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}