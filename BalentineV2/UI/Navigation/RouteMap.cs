// File: UI/Navigation/RouteMap.cs
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

using BalentineV2.UI.Views.Pages;
using BalentineV2.UI.Views.Pages.Modules;

namespace BalentineV2.UI.Navigation;

public sealed class RouteMap
{
    private readonly Dictionary<string, Func<IServiceProvider, ContentPage>> _map =
        new(StringComparer.OrdinalIgnoreCase);

    public RouteMap()
    {
        // Core
        Register(Routes.Home, sp => sp.GetRequiredService<HomeView>());
        Register(Routes.Settings, sp => sp.GetRequiredService<SettingsView>());

        // Modules (opsiyonel)
        Register(Routes.Monitoring, sp => sp.GetRequiredService<MonitoringView>());
        Register(Routes.Fan, sp => sp.GetRequiredService<FanView>());
        Register(Routes.Humidity, sp => sp.GetRequiredService<HumidityView>());
        Register(Routes.Camera, sp => sp.GetRequiredService<CameraView>());
        Register(Routes.Lamp, sp => sp.GetRequiredService<LampView>());
        Register(Routes.Scale, sp => sp.GetRequiredService<ScaleView>());
        Register(Routes.Hydraulic, sp => sp.GetRequiredService<HydraulicView>());
        Register(Routes.Lubrication, sp => sp.GetRequiredService<LubricationView>());
    }

    private void Register(string route, Func<IServiceProvider, ContentPage> factory)
        => _map[route] = factory;

    public ContentPage Resolve(string route, IServiceProvider sp)
    {
        if (_map.TryGetValue(route, out var factory))
            return factory(sp);

        throw new KeyNotFoundException($"Route not found: '{route}'");
    }
}
