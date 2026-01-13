using Microsoft.Maui.Storage;
using BalentineV2.Core.StatusBar;

namespace BalentineV2.Infrastructure.StatusBar;

public sealed class StatusBarConfigStore : IStatusBarConfigStore
{
    private const string Prefix = "statuschip.";

    public event EventHandler? Changed;

    public bool IsEnabled(string chipId)
        => Preferences.Get(Prefix + chipId, true);

    public void SetEnabled(string chipId, bool enabled)
    {
        Preferences.Set(Prefix + chipId, enabled);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
