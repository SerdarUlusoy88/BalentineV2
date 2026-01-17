// File: UI/Navigation/ActivatableHostView.cs
using Microsoft.Maui.Controls;

namespace BalentineV2.UI.Navigation;

/// <summary>
/// ContentHost.Content = page.Content yapısı bozulmadan,
/// page lifecycle yerine custom activation sağlar.
/// </summary>
public sealed class ActivatableHostView : ContentView
{
    public IActivatablePage? Activatable { get; }

    public ActivatableHostView(View content, object? bindingContext, IActivatablePage? activatable)
    {
        Content = content;
        BindingContext = bindingContext;
        Activatable = activatable;
    }
}
