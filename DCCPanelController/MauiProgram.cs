using System.Diagnostics;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Services;
using DCCPanelController.View;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace DCCPanelController;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
               .ConfigureFonts(RegisterAllFonts)
               .ConfigureMauiHandlers(handlers => { })
               .UseSkiaSharp()
               .UseMauiCommunityToolkit()
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
        services.AddSingleton<Profile>(sp => Profile.NewOrLoad("Sample"));
        services.AddSingleton<ConnectionService>();

        // Add dependent views with associated view models
        // --------------------------------------------------------------------------
        services.AddSingletonViewAndModel<AboutPage, AboutViewModel>();
        services.AddSingletonViewAndModel<InstructionsPage, InstructionsViewModel>();
        services.AddSingletonViewAndModel<OperatePage, OperateViewModel>();
        services.AddSingletonViewAndModel<PanelViewer, PanelViewerViewModel>();
        
        services.AddTransientViewAndModel<TurnoutsPage, TurnoutsViewModel>();
        services.AddTransientViewAndModel<RoutesPage, RoutesViewModel>();
        services.AddTransientViewAndModel<BlocksPage, BlocksViewModel>();
        services.AddTransientViewAndModel<SensorsPage, SensorsViewModel>();
        services.AddTransientViewAndModel<LightsPage, LightsViewModel>();
        //services.AddTransientViewAndModel<SignalsPage, LightsViewModel>();

        services.AddTransientViewAndModel<SettingsPage, SettingsPageViewModel>();
        services.AddTransientViewAndModel<ServerMessagesPage, ServerMessagesViewModel>();

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

        //services.AddSingleton<TView>(serviceProvider => new TView { BindingContext = serviceProvider.GetService<TViewModel>() });
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
    
    private static void RegisterAllFonts(IFontCollection fonts) {
        var fontDefinitions = new[] {
            "OpenSans-Regular",
            "OpenSans-Bold",
            "OpenSans-Light",

            "OpenSans-SemiBoldItalic",
            "OpenSans-SemiBold",
            "OpenSans-MediumItalic",
            "OpenSans-Medium",
            "OpenSans-LightItalic",
            "OpenSans-Italic",
            "OpenSans-ExtraBoldItalic",
            "OpenSans-ExtraBold",
            "OpenSans-BoldItalic",
            "OpenSans_SemiCondensed-SemiBoldItalic",
            "OpenSans_SemiCondensed-SemiBold",
            "OpenSans_SemiCondensed-Regular",
            "OpenSans_SemiCondensed-MediumItalic",
            "OpenSans_SemiCondensed-Medium",
            "OpenSans_SemiCondensed-LightItalic",
            "OpenSans_SemiCondensed-Light",
            "OpenSans_SemiCondensed-Italic",
            "OpenSans_SemiCondensed-ExtraBoldItalic",
            "OpenSans_SemiCondensed-ExtraBold",
            "OpenSans_SemiCondensed-BoldItalic",
            "OpenSans_SemiCondensed-Bold",
            "OpenSans_Condensed-SemiBoldItalic",
            "OpenSans_Condensed-SemiBold",
            "OpenSans_Condensed-Regular",
            "OpenSans_Condensed-MediumItalic",
            "OpenSans_Condensed-Medium",
            "OpenSans_Condensed-LightItalic",
            "OpenSans_Condensed-Light",
            "OpenSans_Condensed-Italic",
            "OpenSans_Condensed-ExtraBoldItalic",
            "OpenSans_Condensed-ExtraBold",
            "OpenSans_Condensed-BoldItalic",
            "OpenSans_Condensed-Bold",
            "OpenSans",
        };

        var aliases = new List<string>();
        foreach (var fontBase in fontDefinitions) {
            try {
                var fontFile = $"{fontBase}.ttf";
                var fontName = fontBase.Replace("-", "").Replace("_","");
                if (aliases.Contains(fontName)) continue;
                fonts.AddFont(fontFile, fontName);
                aliases.Add(fontName);
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Failed to register font {fontBase}: {ex.Message}");
            }
        }
    }

}