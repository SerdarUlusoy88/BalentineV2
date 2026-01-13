// File: UI/Navigation/NavigationService.cs
using System;
using Microsoft.Maui.Controls;

namespace BalentineV2.UI.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly RouteMap _routeMap;

    private ContentView? _host;

    public string? CurrentRoute { get; private set; }

    public event EventHandler<string>? RouteChanged;

    public NavigationService(IServiceProvider services, RouteMap routeMap)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _routeMap = routeMap ?? throw new ArgumentNullException(nameof(routeMap));
    }

    public void SetContentHost(ContentView host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void NavigateTo(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route cannot be null/empty.", nameof(route));

        if (_host is null)
            throw new InvalidOperationException("ContentHost not set. Call SetContentHost() first.");

        // Aynı route'a tekrar gitmeyi engelle (gereksiz re-render)
        if (string.Equals(CurrentRoute, route, StringComparison.OrdinalIgnoreCase))
            return;

        var page = _routeMap.Resolve(route, _services);

        if (page.Content is null)
            throw new InvalidOperationException($"Resolved page '{route}' has null Content.");

        // ✅ ContentView sadece View alır → ContentPage.Content basıyoruz
        _host.Content = page.Content;

        // BindingContext'i de host'a taşıyalım (sayfa içinde VM vs varsa bozulmasın)
        _host.BindingContext = page.BindingContext;

        CurrentRoute = route;
        RouteChanged?.Invoke(this, route);
    }
}
