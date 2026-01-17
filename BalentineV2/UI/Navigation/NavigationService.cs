using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace BalentineV2.UI.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private readonly RouteMap _routeMap;

    private ContentView? _host;

    private ContentPage? _currentPage;
    private IActivatablePage? _currentActivatable;

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
        Debug.WriteLine($"[NAV] NavigateTo('{route}')");

        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route cannot be null/empty.", nameof(route));

        if (_host is null)
            throw new InvalidOperationException("ContentHost not set. Call SetContentHost() first.");

        if (string.Equals(CurrentRoute, route, StringComparison.OrdinalIgnoreCase))
        {
            Debug.WriteLine("[NAV] Same route, skip.");
            return;
        }

        // 1) Resolve
        var page = _routeMap.Resolve(route, _services);

        if (page.Content is null)
            throw new InvalidOperationException($"Resolved page '{route}' has null Content.");

        Debug.WriteLine($"[NAV] Resolved page: {page.GetType().FullName}");
        Debug.WriteLine($"[NAV] Resolved content: {page.Content.GetType().FullName}");

        // 2) Deactivate old
        try
        {
            if (_currentActivatable is not null)
            {
                Debug.WriteLine($"[NAV] Deactivate: {_currentActivatable.GetType().FullName}");
                _currentActivatable.OnDeactivated();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NAV] Deactivate ERROR: {ex}");
        }

        // 3) Host'a bas (mimari aynı)
        _host.Content = page.Content;
        _host.BindingContext = page.BindingContext;

        _currentPage = page;
        CurrentRoute = route;

        RouteChanged?.Invoke(this, route);

        // 4) Activatable bul (page üzerinden)
        var activatable = page as IActivatablePage;
        Debug.WriteLine($"[NAV] Activatable found? {activatable is not null}");

        _currentActivatable = activatable;

        if (activatable is not null)
            _ = ActivateAsync(activatable);
    }

    private static async Task ActivateAsync(IActivatablePage page)
    {
        try
        {
            // ContentHost'a basıldıktan sonra 2 frame bekle
            await Task.Delay(50);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Task.Delay(150);
                Debug.WriteLine($"[NAV] Activate: {page.GetType().FullName}");
                await page.OnActivatedAsync();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NAV] Activate ERROR: {ex}");
        }
    }
}
