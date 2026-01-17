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
using BalentineV2.UI.Controls;

// DİKKAT: "using LibVLCSharp..." BURADAN SİLİNDİ.

#if ANDROID
using BalentineV2.Platforms.Android.Handlers;
#endif

namespace BalentineV2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

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
            .UseMauiCommunityToolkitMediaElement() // Bu kalabilir, zararı yok.
                                                   // .UseLibVLCSharp()  <-- İŞTE HATAYI YAPAN SATIR BU! BUNU SİLDİK.
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- SERVİSLER ---
        builder.Services.AddSingleton<IFeatureConfigStore, FeatureConfigStore>();
        builder.Services.AddSingleton<IFeatureConfigService, FeatureConfigService>();
        builder.Services.AddSingleton<IMenuProvider, MenuProvider>();
        builder.Services.AddSingleton<IStatusBarProvider, StatusBarProvider>();
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        builder.Services.AddSingleton<Loc>();
        builder.Services.AddSingleton<ITextProvider, ResxTextProvider>();
        builder.Services.AddSingleton<RouteMap>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IRailStateService, RailStateService>();
        builder.Services.AddSingleton<IAppSettingsStore, AppSettingsStore>();

        // --- SAYFALAR & GÖRÜNÜMLER ---
        builder.Services.AddSingleton<IconRailView>();
        builder.Services.AddSingleton<TopStatusBarView>();
        builder.Services.AddTransient<LanguageSelectorView>();
        builder.Services.AddSingleton<MainLayout>();
        builder.Services.AddSingleton<SettingsView>();

        builder.Services.AddSingleton<HomeView>(); // Video burada
        builder.Services.AddSingleton<MonitoringView>();
        builder.Services.AddSingleton<FanView>();
        builder.Services.AddSingleton<HumidityView>();
        builder.Services.AddSingleton<CameraView>();
        builder.Services.AddSingleton<LampView>();
        builder.Services.AddSingleton<ScaleView>();
        builder.Services.AddSingleton<HydraulicView>();
        builder.Services.AddSingleton<LubricationView>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}