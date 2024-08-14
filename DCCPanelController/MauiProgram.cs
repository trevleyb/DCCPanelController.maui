using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using DCCPanelController.Services;
using DCCPanelController.View;
using DCCPanelController.ViewModel;

namespace DCCPanelController;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts => {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();

        var services = builder.Services;
        
        // Add dependant Services
        // --------------------------------------------------------------------------
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ConnectionService>();
        services.AddSingleton<RoutesService>();
        services.AddSingleton<TurnoutsService>();
        
        // Register the Main Entry Page that we will use 
        // --------------------------------------------------------------------------
        services.AddSingleton<MainPageFlyOut>();
        
        // Add dependant views with associated view models
        // --------------------------------------------------------------------------
        services.AddSingletonViewAndModel<SettingsPage, SettingsViewModel>();
        services.AddSingletonViewAndModel<PanelsPage, PanelsViewModel>();
        services.AddSingletonViewAndModel<OperatePage, OperateViewModel>();
        services.AddSingletonViewAndModel<AboutPage, AboutViewModel>();
        services.AddSingletonViewAndModel<InstructionsPage, InstructionsViewModel>();
        
        services.AddSingletonViewAndModel<RoutesPage, RoutesViewModel>();
        services.AddSingletonViewAndModel<TurnoutsPage, TurnoutsViewModel>();
        
        services.AddTransient<PanelEditorPage>();
        services.AddTransient<PanelEditorViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        var app = builder.Build();

        //var xx1 = app.Services.GetRequiredService<RoutesPage>();
        //var xx2 = app.Services.GetRequiredService<RoutesService>();
        
        // Assign the service provider to a static property
        App.ServiceProvider = app.Services;
        return app;
    }

    /// <summary>
    /// Registers a transient view with its associated view model in the service collection.
    /// </summary>
    /// <typeparam name="TView">The type of the view to be registered.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model associated with the view.</typeparam>
    /// <param name="services">The service collection to register the view and view model.</param>
    private static void AddTransientViewAndModel<TView, TViewModel>(this IServiceCollection services) where TView : ContentPage, new() where TViewModel : class {
        services.AddTransient<TViewModel>();
        services.AddTransient<TView>(serviceProvider => new TView { BindingContext = serviceProvider.GetService<TViewModel>() });
    }

    /// <summary>
    /// Registers a singleton view with its associated view model in the service collection.
    /// </summary>
    /// <typeparam name="TView">The type of the view to be registered.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model associated with the view.</typeparam>
    /// <param name="services">The service collection to register the view and view model.</param>
    private static void AddSingletonViewAndModel<TView, TViewModel>(this IServiceCollection services) where TView : ContentPage, new() where TViewModel : class {
        services.AddSingleton<TViewModel>();
        services.AddSingleton<TView>(serviceProvider => new TView { BindingContext = serviceProvider.GetService<TViewModel>() });
    }

    /// <summary>
    /// Registers a view route in the Maui routing system based on the specified view type.
    /// </summary>
    /// <typeparam name="TView">The type of the view to register the route for.</typeparam>
    private static void RegisterViewRoute<TView>() {
        // Remove the suffix (Page or View) from the name of the View
        var routeName = typeof(TView).Name.Replace("Page", "").Replace("View", "");
        Routing.RegisterRoute(routeName, typeof(TView));
        Console.WriteLine($"Registered Route: '{routeName}' to '{typeof(TView).Name}'");
    }
} 