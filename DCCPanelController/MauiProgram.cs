using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using DCCPanelController.Helpers;
using DCCPanelController.View;
using Microsoft.Extensions.Logging;

namespace DCCPanelController;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
               .ConfigureFonts(fonts => {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
               .UseMauiCommunityToolkit()
               .ConfigureMauiHandlers(handlers => { })
               .UseMauiCommunityToolkit()
               .UseMauiCommunityToolkitMarkup()
               .UseMauiCommunityToolkitMediaElement();

        FormHelper.RemoveBorders();

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        var services = builder.Services;

        // Register the Main Entry Page that we will use 
        // --------------------------------------------------------------------------
        services.AddSingleton<MainPageTabbed>();

        // Add dependant Services
        // --------------------------------------------------------------------------
        //services.AddSingleton<ConnectionService>();
        //services.AddTransient<RoutesService>();
        //services.AddTransient<TurnoutsService>();

        // Add dependent views with associated view models
        // --------------------------------------------------------------------------
        services.AddSingletonViewAndModel<AboutPage, AboutViewModel>();
        services.AddSingletonViewAndModel<InstructionsPage, InstructionsViewModel>();

        var app = builder.Build();
        ServiceHelper.Initialize(app.Services);
        LogHelper.Initialize(app.Services.GetRequiredService<ILoggerFactory>());
        
        return app;
    }

    /// <summary>
    ///     Registers a transient view with its associated view model in the service collection.
    /// </summary>
    /// <typeparam name="TView">The type of the view to be registered.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model associated with the view.</typeparam>
    /// <param name="services">The service collection to register the view and view model.</param>
    private static void AddTransientViewAndModel<TView, TViewModel>(this IServiceCollection services) where TView : ContentPage, new() where TViewModel : class {
        services.AddTransient<TViewModel>();
        services.AddTransient<TView>(serviceProvider => new TView { BindingContext = serviceProvider.GetService<TViewModel>() });
    }

    /// <summary>
    ///     Registers a singleton view with its associated view model in the service collection.
    /// </summary>
    /// <typeparam name="TView">The type of the view to be registered.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model associated with the view.</typeparam>
    /// <param name="services">The service collection to register the view and view model.</param>
    private static void AddSingletonViewAndModel<TView, TViewModel>(this IServiceCollection services) where TView : ContentPage, new() where TViewModel : class {
        services.AddSingleton<TViewModel>();
        services.AddSingleton<TView>(serviceProvider => new TView { BindingContext = serviceProvider.GetService<TViewModel>() });
    }

    /// <summary>
    ///     Registers a view route in the Maui routing system based on the specified view type.
    /// </summary>
    /// <typeparam name="TView">The type of the view to register the route for.</typeparam>
    private static void RegisterViewRoute<TView>() {
        // Remove the suffix (Page or View) from the name of the View
        var routeName = typeof(TView).Name.Replace("Page", "").Replace("View", "");
        Routing.RegisterRoute(routeName, typeof(TView));
    }

    public static class ServiceHelper {
        private static IServiceProvider? ServiceProvider { get; set; }

        public static void Initialize(IServiceProvider serviceProvider) {
            ServiceProvider = serviceProvider;
        }

        public static T GetService<T>() where T : notnull {
            ArgumentNullException.ThrowIfNull(ServiceProvider);
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}