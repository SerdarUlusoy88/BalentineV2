using BalentineV2.Core.Features;
using BalentineV2.Core.Localization;
using BalentineV2.Core.Menu;
using BalentineV2.Core.Settings;
using BalentineV2.Core.StatusBar;
using BalentineV2.Infrastructure.Features;
using BalentineV2.Infrastructure.Localization;
using BalentineV2.Infrastructure.Menu;
using BalentineV2.Infrastructure.Settings;
using BalentineV2.Infrastructure.StatusBar;
using BalentineV2.UI.Components.Bars;
using BalentineV2.UI.Components.Navigation;
using BalentineV2.UI.Localization;
using BalentineV2.UI.Navigation;
using BalentineV2.UI.Views.Main;
using BalentineV2.UI.Views.Pages;
using BalentineV2.UI.Views.Pages.Modules;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

// 🔴 EK: AHD Camera control + handler
using BalentineV2.UI.Controls;

#if ANDROID
using BalentineV2.Platforms.Android.Handlers;
#endif

namespace BalentineV2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // --------------------------------------------------------------------
        // APP BOOTSTRAP
        // --------------------------------------------------------------------
        builder
    .UseMauiApp<App>()
    .ConfigureMauiHandlers(handlers =>
    {
#if ANDROID
        handlers.AddHandler(typeof(BalentineV2.UI.Controls.AhdCameraView),
                            typeof(BalentineV2.Platforms.Android.Handlers.AhdCameraViewHandler));
#endif
    })
    .UseMauiCommunityToolkit()
    .UseMauiCommunityToolkitMediaElement()
    .ConfigureFonts(fonts =>
    {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
    });

        // --------------------------------------------------------------------
        // CORE / INFRASTRUCTURE SERVICES
        // --------------------------------------------------------------------

        // Feature Config (Module enable/disable vb.)
        builder.Services.AddSingleton<IFeatureConfigStore, FeatureConfigStore>();
        builder.Services.AddSingleton<IFeatureConfigService, FeatureConfigService>();

        // Menu provider
        builder.Services.AddSingleton<IMenuProvider, MenuProvider>();

        // Status bar chip provider
        builder.Services.AddSingleton<IStatusBarProvider, StatusBarProvider>();

        // Localization
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        builder.Services.AddSingleton<Loc>();
        builder.Services.AddSingleton<ITextProvider, ResxTextProvider>();

        // --------------------------------------------------------------------
        // NAVIGATION
        // --------------------------------------------------------------------
        builder.Services.AddSingleton<RouteMap>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // IconRail state
        builder.Services.AddSingleton<IRailStateService, RailStateService>();

        // --------------------------------------------------------------------
        // APP SETTINGS (GLOBAL)
        // --------------------------------------------------------------------
        builder.Services.AddSingleton<IAppSettingsStore, AppSettingsStore>();

        // --------------------------------------------------------------------
        // UI - COMPONENTS
        // --------------------------------------------------------------------
        builder.Services.AddTransient<IconRailView>();
        builder.Services.AddTransient<TopStatusBarView>();
        builder.Services.AddTransient<LanguageSelectorView>();

        // --------------------------------------------------------------------
        // UI - LAYOUT
        // --------------------------------------------------------------------
        builder.Services.AddTransient<MainLayout>();

        // --------------------------------------------------------------------
        // UI - PAGES
        // --------------------------------------------------------------------
        builder.Services.AddTransient<SettingsView>();

        // Modules
        builder.Services.AddTransient<HomeView>();
        builder.Services.AddTransient<MonitoringView>();
        builder.Services.AddTransient<FanView>();
        builder.Services.AddTransient<HumidityView>();
        builder.Services.AddTransient<CameraView>();
        builder.Services.AddTransient<LampView>();
        builder.Services.AddTransient<ScaleView>();
        builder.Services.AddTransient<HydraulicView>();
        builder.Services.AddTransient<LubricationView>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
