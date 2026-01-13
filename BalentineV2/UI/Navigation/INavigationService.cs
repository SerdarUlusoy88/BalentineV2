using System;
using Microsoft.Maui.Controls;

namespace BalentineV2.UI.Navigation;

public interface INavigationService
{
    string? CurrentRoute { get; }
    event EventHandler<string>? RouteChanged;

    // ✅ Senin projede beklenen isim
    void SetContentHost(ContentView host);

    void NavigateTo(string route);
}
