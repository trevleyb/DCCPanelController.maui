using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using DCCPanelController.Helpers;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using DCCPanelController.View;
using DCCPanelController.View.Components;
using Microsoft.Extensions.Logging;
using Serilog;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Handlers.Items2;
#endif

namespace DCCPanelController;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        ConfigureSerilog();
        builder.UseMauiApp<App>()
               .ConfigureMauiHandlers(handlers => { })
               .UseSkiaSharp()
               .ConfigureSyncfusionToolkit()
               .UseMauiCommunityToolkit()
               .UseMauiCommunityToolkitMarkup()
               .UseMauiCommunityToolkitMediaElement()
               .ConfigureMauiHandlers(handlers => {
                    #if IOS || MACCATALYST
                    handlers.AddHandler<CollectionView, CollectionViewHandler2>();
                    #endif
                })
               .ConfigureFonts(fonts => {
                    fonts.AddFont("OpenSans.ttf", "OpenSans");
                    fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
                    fonts.AddFont("OpenSans-BoldItalic.ttf", "OpenSansBoldItalic");
                    fonts.AddFont("OpenSans-ExtraBold.ttf", "OpenSansExtraBold");
                    fonts.AddFont("OpenSans-ExtraBoldItalic.ttf", "OpenSansExtraBoldItalic");
                    fonts.AddFont("OpenSans-Italic.ttf", "OpenSansItalic");
                    fonts.AddFont("OpenSans-Light.ttf", "OpenSansLight");
                    fonts.AddFont("OpenSans-LightItalic.ttf", "OpenSansLightItalic");
                    fonts.AddFont("OpenSans-Medium.ttf", "OpenSansMedium");
                    fonts.AddFont("OpenSans-MediumItalic.ttf", "OpenSansMediumItalic");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
                    fonts.AddFont("OpenSans-SemiBoldItalic.ttf", "OpenSansSemiBoldItalic");
                });

        FormHelper.RemoveBorders();
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders().AddSerilog(dispose: true));

        #if DEBUG
        builder.Logging.AddDebug();
        #endif

        var services = builder.Services;

        // Register the Main Entry Page that we will use 
        // --------------------------------------------------------------------------
        services.AddSingleton<AppShell>();

        // Add dependant Services
        // --------------------------------------------------------------------------
        services.AddSingleton<ProfileService>();
        services.AddSingleton<AppStateService>();
        services.AddSingleton<ConnectionService>();
        services.AddSingleton<HelpPage>();
        services.AddSingleton<AboutPage>();

        // Add dependent views with associated view models
        // --------------------------------------------------------------------------
        services.AddSingletonViewAndModel<OperatePage, OperateViewModel>();
        services.AddSingletonViewAndModel<PanelViewer, PanelViewerViewModel>();
        services.AddSingletonViewAndModel<TestPage, TestPageViewModel>();

        services.AddTransientViewAndModel<TurnoutsPage, TurnoutsViewModel>();
        services.AddTransientViewAndModel<RoutesPage, RoutesViewModel>();
        services.AddTransientViewAndModel<BlocksPage, BlocksViewModel>();
        services.AddTransientViewAndModel<SensorsPage, SensorsViewModel>();
        services.AddTransientViewAndModel<LightsPage, LightsViewModel>();

        //services.AddTransientViewAndModel<SignalsPage, LightsViewModel>();

        services.AddTransientViewAndModel<SettingsPage, SettingsPageViewModel>();
        services.AddTransientViewAndModel<ServerMessagesPage, ServerMessagesViewModel>();
        services.AddTransientPopup<ColorPickerGrid, ColorPickerGridViewModel>();

        Routing.RegisterRoute("help", typeof(HelpPage));

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
    private static void AddTransientViewAndModel<TView, TViewModel>(this IServiceCollection services) where TView : ContentPage where TViewModel : class {
        services.AddTransient<TViewModel>();

        //services.AddTransient<TView>(serviceProvider => new TView { BindingContext = serviceProvider.GetService<TViewModel>() });
        services.AddTransient<TView>();
    }

    /// <summary>
    ///     Registers a singleton view with its associated view model in the service collection.
    /// </summary>
    /// <typeparam name="TView">The type of the view to be registered.</typeparam>
    /// <typeparam name="TViewModel">The type of the view model associated with the view.</typeparam>
    /// <param name="services">The service collection to register the view and view model.</param>
    private static void AddSingletonViewAndModel<TView, TViewModel>(this IServiceCollection services) where TView : ContentPage where TViewModel : class {
        services.AddSingleton<TViewModel>();
        services.AddSingleton<TView>();
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

    private static void ConfigureSerilog() {
        var logFilePath = GetLogFilePath();
        var levelSwitch = LoggingLevelHelper.Initialize();
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .WriteTo.File(
                         logFilePath,
                         rollingInterval: RollingInterval.Day,
                         retainedFileCountLimit: 7,
                         fileSizeLimitBytes: 10_000_000,
                         rollOnFileSizeLimit: true,
                         shared: true,
                         outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                     #if DEBUG
                    .WriteTo.Debug()
                    .WriteTo.Console()
                     #endif
                    .CreateLogger();

        Log.Information("Logger Initialised.");
    }

    private static string GetLogFilePath() => Path.Combine(LogHelper.GetLogDirectory(), "dccpanelcontroller.log");

    public static class ServiceHelper {
        private static IServiceProvider? ServiceProvider { get; set; }

        public static void Initialize(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;

        public static T GetService<T>() where T : notnull {
            ArgumentNullException.ThrowIfNull(ServiceProvider);
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}