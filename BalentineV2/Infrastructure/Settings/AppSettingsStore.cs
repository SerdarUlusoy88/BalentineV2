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

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AppSettings Current { get; private set; } = new();

    public event EventHandler<AppSettings>? Changed;

    public AppSettings Load()
    {
        AppSettings loaded;

        try
        {
            var json = Preferences.Get(PrefKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                loaded = new AppSettings();
            }
            else
            {
                loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch
        {
            loaded = new AppSettings();
        }

        // ✅ Her koşulda normalize
        loaded = loaded.Normalize();

        // ✅ Current güncelle
        Current = loaded;

        // ✅ UI varsa dinleyicilere bildir (main thread)
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Changed?.Invoke(this, Current);
        });

        return Current;
    }

    public void Save(AppSettings settings)
    {
        if (settings is null)
            settings = new AppSettings();

        // ✅ Normalize ederek kaydet
        var normalized = settings.Normalize();

        // ✅ Current anında güncellensin
        Current = normalized;

        // ✅ UI güncellemesi anında gelsin (kamera bölünmesi + mirror)
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Changed?.Invoke(this, Current);
        });

        // ✅ Preferences yazımı arka planda devam etsin
        _ = Task.Run(() =>
        {
            try
            {
                var json = JsonSerializer.Serialize(Current, JsonOptions);
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
        camera ??= new CameraSettings();

        // ✅ CameraSettings normalize (count clamp + mirror2 rule)
        var normalizedCamera = camera.Normalize();

        Save(Current with { Camera = normalizedCamera });
    }
}
