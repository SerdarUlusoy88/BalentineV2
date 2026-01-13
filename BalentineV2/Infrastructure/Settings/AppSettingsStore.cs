using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

using BalentineV2.Core.Settings;

namespace BalentineV2.Infrastructure.Settings;

public sealed class AppSettingsStore : IAppSettingsStore
{
    private const string PrefKey = "app_settings_v1";

    public AppSettings Current { get; private set; } = new();

    public event EventHandler<AppSettings>? Changed;

    public AppSettings Load()
    {
        try
        {
            var json = Preferences.Get(PrefKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                Current = new AppSettings();
                return Current;
            }

            var cfg = JsonSerializer.Deserialize<AppSettings>(json);
            Current = cfg ?? new AppSettings();
            return Current;
        }
        catch
        {
            Current = new AppSettings();
            return Current;
        }
    }

    public void Save(AppSettings settings)
    {
        // ✅ Current anında güncellensin
        Current = settings;

        // ✅ UI güncellemesi anında gelsin (kamera bölünmesi + mirror)
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Changed?.Invoke(this, settings);
        });

        // ✅ Preferences yazımı arka planda devam etsin
        _ = Task.Run(() =>
        {
            try
            {
                var json = JsonSerializer.Serialize(settings);
                Preferences.Set(PrefKey, json);
            }
            catch
            {
                // sessiz geç
            }
        });
    }


    public void SaveCamera(CameraSettings camera)
    {
        Save(Current with { Camera = camera });
    }
}
